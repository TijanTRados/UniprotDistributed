﻿using System;
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

        public string Search()
        {

            return "";
        }

        public async Task<string> Load(string path)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync("http://proteinreader.bioinfo.pbf.hr" + "/master/load?path=" + path);
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

        public async Task<string> Check_Status()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync("http://proteinreader.bioinfo.pbf.hr" + "/master/info");
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

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}