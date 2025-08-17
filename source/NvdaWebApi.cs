using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace NVDASpeechWebApi
{
	class NvdaWebApi : WebApiController
	{
		[Route(EmbedIO.HttpVerbs.Get, "/nvda/check_running")]
		public async Task<object> NVDACheckRunning()
		{
			object returnedValue;
			try
			{
				uint pid = NvdaApi.GetNvdaPid();
				returnedValue = new
				{
					status = 0,
					message = "Success",
					result = new
					{
						isRunning = (pid != 0),
						processId = pid
					}
				};
			}
			catch (Exception ex)
			{
				returnedValue = new
				{
					status = 10,
					message = ex.Message,
					result = default(object)
				};
			}
			return (returnedValue);
		}

		[Route(EmbedIO.HttpVerbs.Post, "/nvda/cancel_speech")]
		public async Task<object> NvdaCancelSpeech()
		{
			object returnedValue;
			try
			{
				int callResult = NvdaApi.CancelSpeech();
				returnedValue = new
				{
					status = 0,
					message = "Success",
					result = new
					{
						code = callResult,
						description = (callResult > 0 ? "NVDA is not running" : "Request submitted")
					}
				};
			}
			catch (Exception ex)
			{
				returnedValue = new
				{
					status = 10,
					message = ex.Message,
					result = default(object)
				};
			}
			return (returnedValue);
		}

		[Route(EmbedIO.HttpVerbs.Post, "/nvda/speak_text")]
		public async Task<object> Nvda_SpeakText()
		{
			object returnedValue;
			try
			{
				TextRequest request = await this.HttpContext.GetRequestDataAsync<TextRequest>();
				int callResult = NvdaApi.NvdaSpeakText(request.text);
				returnedValue = new
				{
					status = 0,
					message = "Success",
					result = new
					{
						code = callResult,
						description = (callResult > 0 ? "NVDA is not running" : "Request submitted")
					}
				};
			}
			catch (Exception ex)
			{
				returnedValue = new
				{
					status = 10,
					message = ex.Message,
					result = default (object)
				};
			}
			return (returnedValue);
		}

		[Route(EmbedIO.HttpVerbs.Post, "/nvda/show_braille-message")]
		public async Task<object> Nvda_ShowBrailleMessage()
		{
			object returnedValue;
			try
			{
				TextRequest request = await this.HttpContext.GetRequestDataAsync<TextRequest>();
				int callResult = NvdaApi.ShowBrailleMessage(request.text);
				returnedValue = new
				{
					status = 0,
					message = "Success",
					result = new
					{
						code = callResult,
						description = (callResult > 0 ? "NVDA is not running" : "Request submitted")
					}
				};
			}
			catch (Exception ex)
			{
				returnedValue = new
				{
					status = 10,
					message = ex.Message,
					result = default(object)
				};
			}
			return (returnedValue);
		}

		class TextRequest
		{
			public string text
			{
				get; set;
			}
		}
	}
}
