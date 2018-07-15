using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using System.Net.Http;
using System.IO;
using System.Text;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {

        string SERVER = "http://storage.bioinfo.pbf.hr";

        public string Search()
        {

            return "";
        }

        [HttpGet]
        public async Task<string> Get(string sql)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    string sql2correct = sql.Replace(" ", "%20");

                    string dothis = SERVER + "/master/get?sql=" + sql2correct;

                    HttpResponseMessage response = await client.GetAsync(SERVER + "/master/get?sql=" + sql2correct);
                    if (response.IsSuccessStatusCode)
                    {
                        Stream receiveStream = await response.Content.ReadAsStreamAsync();
                        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        string result = readStream.ReadToEnd();
                        return result;
                    }
                    else return "{ "+ response.StatusCode + " - " + response.Content +" }";
                }
                catch (Exception ex)
                {
                    return "{ " + ex.Message+ " }";
                }
            }
        }

        [HttpGet]
        public async Task<string> Load(string path)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(SERVER + "/master/load?path=" + path);
                    if (response.IsSuccessStatusCode)
                    {
                        Stream receiveStream = await response.Content.ReadAsStreamAsync();
                        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        string result = readStream.ReadToEnd();
                        return result;
                    }
                    else return "Check if the master server is running";
                }
                catch (Exception)
                {
                    return "Check if the master server is running";
                }
            }
        }

        public async Task<string> Kill()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(SERVER + "/master/kill");
                    if (response.IsSuccessStatusCode)
                    {
                        Stream receiveStream = await response.Content.ReadAsStreamAsync();
                        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        string result = readStream.ReadToEnd();
                        return result;
                    }
                    else return "{Check if the master server is running}";
                }
                catch (Exception)
                {
                    return "{Check if the master server is running}";
                }
            }
        }

        public async Task<string> Check_Status()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(SERVER + "/master/info");
                    if (response.IsSuccessStatusCode)
                    {
                        Stream receiveStream = await response.Content.ReadAsStreamAsync();
                        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        string result = readStream.ReadToEnd();
                        return result;
                    }
                    else return "Check if the master server is running";
                }
                catch (Exception)
                {
                    return "Check if the master server is running";
                }
            }
        }

        public async Task<string> Check_Slaves()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(SERVER + "/master/check_slaves");
                    if (response.IsSuccessStatusCode)
                    {
                        Stream receiveStream = await response.Content.ReadAsStreamAsync();
                        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        string result = readStream.ReadToEnd();
                        return result;
                    }
                    else return "Check if the master server is running";
                }
                catch (Exception)
                {
                    return "Check if the master server is running";
                }
            }
        }

        public async Task<string> Check_Bulk_Status()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(SERVER + "/master/check_bulk_status");
                    if (response.IsSuccessStatusCode)
                    {
                        Stream receiveStream = await response.Content.ReadAsStreamAsync();
                        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        string result = readStream.ReadToEnd();
                        return result;
                    }
                    else return "Check if the master server is running";
                }
                catch (Exception)
                {
                    return "Check if the master server is running";
                }
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Query()
        {
            ViewData["Message"] = "Query";

            return View();
        }

        public IActionResult Rebalance()
        {
            ViewData["Message"] = "Rebalance";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
