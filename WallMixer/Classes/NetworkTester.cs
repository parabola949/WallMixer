namespace WallMixer.Classes
{
    using System.Net.NetworkInformation;
    using System.Threading.Tasks;

    public static class NetworkTester
    {
        public static async Task<bool> HasInternetAccess()
        {
            try
            {
                var result = await PingAsync("www.google.com");
                return result.Status == IPStatus.Success;
            }
            catch { return false; }
        }

        private static Task<PingReply> PingAsync(string address)
        {
            var taskcs = new TaskCompletionSource<PingReply>();
            using (var ping = new Ping())
            {
                ping.PingCompleted += (obj, sender) => taskcs.SetResult(sender.Reply);
                ping.SendAsync(address, 100, new object());
            }
            return taskcs.Task;
        }
    }
}
