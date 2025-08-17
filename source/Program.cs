using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.WebApi;

namespace NVDASpeechWebApi
{
	static class Program
	{
		static int ListeningPoint = 12320;

		static async Task<int> Main(string[] args)
		{
			try
			{
				Dictionary<string, List<string>> cmdArgs = ParseArgs(args);
				ListeningPoint = (cmdArgs.TryGetValue("-p",out var newPoint) ? int.Parse(newPoint.FirstOrDefault()):ListeningPoint);
				await RunWebService();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				return (-1);
			}
			return (0);
		}

		static Dictionary<string, List<string>> ParseArgs(string[] args)
		{
			Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
			string currentArgName = "";
			result[currentArgName] = new List<string>();
			foreach (string arg in args)
			{
				string tmpArg = arg.Trim();
				if (tmpArg.StartsWith("-") || arg.StartsWith("+"))
				{
					currentArgName = tmpArg;
					result[currentArgName] = (result.TryGetValue(currentArgName,out var argList)?argList:new List<string>());
					continue;
				}
				result[currentArgName].Add(arg);
			}
			return (result);
		}

		static async Task RunWebService()
		{
			using (WebServer server = new WebServer((_o) =>
			{
				_o.AddUrlPrefix($"http://*:{ListeningPoint}");
				_o.WithMode(HttpListenerMode.EmbedIO);
			}))
			{
				server.HandleHttpException(async (context, exception) =>
				{
					await HandleErrorAsync(context, exception.StatusCode, context.Response.StatusDescription ?? exception.Message);
				});
				server.HandleUnhandledException(async (context, exception) =>
				{
					await HandleErrorAsync(context, (int)HttpStatusCode.InternalServerError, exception.Message ?? "An unknown error has occurred");
				});
				server.WithCors();
				server.WithWebApi("/api", (_c) =>
				{
					_c.WithController(typeof(NvdaWebApi));
				});
				server.WithWebApi("/lib", (_c) =>
				{
					_c.WithController(typeof(StaticContentController));
				});
				await server.RunAsync();
			}
		}

		static async Task HandleErrorAsync(IHttpContext context, int statusCode, string message)
		{
			context.Response.StatusCode = statusCode;
			var responsePayload = new
			{
				status = statusCode,
				message = message,
				result = default(object)
			};
			await context.SendDataAsync(responsePayload);
		}
	}
}
