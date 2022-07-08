using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace ActiveWinTest {
    class Program {
        //https://stackoverflow.com/questions/115868/how-do-i-get-the-title-of-the-current-active-window-using-c

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowTextLength(IntPtr hWnd);
        
        static async Task Main(string[] args) {
            Console.WriteLine("Running!");

            string oldTitle = "";
            const int nChars = 256;
            int sleepDuration = 500;
            StringBuilder Buff = new StringBuilder(nChars);
            while (true) {
                IntPtr handle = GetForegroundWindow();
                string currentTitle = "";
                
                if (GetWindowText(handle, Buff, nChars) > 0) {
                    currentTitle = Buff.ToString();
                }

                if (currentTitle != oldTitle && currentTitle != "") {
                    oldTitle = currentTitle;
                    Console.WriteLine(currentTitle);
                    await SendPostRequest("http://localhost:3456/updateActiveWin", currentTitle);
                }

                Thread.Sleep(sleepDuration);
            }
        }

        private static async Task<string> SendPostRequest(string url, string activeWindow) {
            try {
                var body = new Dictionary<string, string> {
                    { "windowTitle", activeWindow }
                };

                var content = JsonSerializer.Serialize(body);
                HttpResponseMessage response = null;
                using (var client = new HttpClient()) {
                    response = await client.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
                }

                string result = response.Content.ReadAsStringAsync().Result;
                return result;
            } catch (Exception ex) {
                Console.WriteLine("Error in SendPostRequest");
                return "";
            }
        }
    }
}
