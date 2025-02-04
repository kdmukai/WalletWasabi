using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WalletWasabi.Bases;
using WalletWasabi.Helpers;
using WalletWasabi.Logging;

namespace WalletWasabi.BitcoinCore.Monitoring
{
	public class P2pReconnector : PeriodicRunner<bool>
	{
		public P2pNode P2pNode { get; set; }

		public P2pReconnector(TimeSpan period, P2pNode p2pNode) : base(period, false)
		{
			P2pNode = Guard.NotNull(nameof(p2pNode), p2pNode);
		}

		protected override async Task<bool> ActionAsync(CancellationToken cancel)
		{
			try
			{
				Logger.LogInfo("Trying to reconnect to P2P...");
				P2pNode.Disconnect();
				await P2pNode.ConnectAsync(cancel).ConfigureAwait(false);
			}
			catch
			{
				P2pNode.Disconnect();
				throw;
			}

			Logger.LogInfo("Successfully reconnected to P2P.");
			return true;
		}

		public async Task StartAndAwaitReconnectionAsync(CancellationToken cancel)
		{
			Stop?.Dispose();
			Stop = CancellationTokenSource.CreateLinkedTokenSource(cancel);
			ForeverTask = ForeverMethodAsync(x => x is true);
			await ForeverTask.ConfigureAwait(false);
			await StopAsync().ConfigureAwait(false);
		}
	}
}
