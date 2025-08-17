using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace NVDASpeechWebApi
{
	public class StaticContentController : WebApiController
	{
		private const string NvdaWebApiJs = @"
(function(window) {
    'use strict';
    const scriptUrl = new URL(document.currentScript.src);
    const BASE_URL = `${scriptUrl.protocol}//${scriptUrl.host}/api/nvda`;

    console.log(`NVDA_API library initialized. Targeting API server at: ${BASE_URL}`);

    async function _fetchAPI(endpoint, options = {}) {
        try {
            const response = await fetch(BASE_URL + endpoint, options);

            if (!response.ok) {
                console.error(`NVDA_API Error: HTTP status ${response.status} for endpoint ${endpoint}`);
                return { success: false, data: null };
            }

            const jsonResponse = await response.json();

            if (jsonResponse.status === 0 && jsonResponse.result) {
                if (typeof jsonResponse.result.code !== 'undefined' && jsonResponse.result.code !== 0) {
                     console.warn(`NVDA_API Info: Command failed. Code: ${jsonResponse.result.code}, Description: ${jsonResponse.result.description}`);
                     return { success: false, data: jsonResponse };
                }
                return { success: true, data: jsonResponse };
            } else {
                console.error(`NVDA_API Error: Server returned failure status. Response:`, jsonResponse);
                return { success: false, data: jsonResponse };
            }
        } catch (error) {
            console.error(`NVDA_API Fatal Error: A network or parsing error occurred for endpoint ${endpoint}.`, error);
            return { success: false, data: null };
        }
    }

    const NVDA_API = {
        /**
         * Checks if NVDA is running.
         * @returns {Promise<number>} - A promise that resolves to the NVDA process ID (PID) if running, otherwise 0.
         */
        async checkRunning() {
            const result = await _fetchAPI('/check_running', { method: 'GET' });
            if (result.success && result.data.result.isRunning) {
                return result.data.result.processId;
            }
            return 0;
        },

        /**
         * Requests NVDA to speak the given text.
         * @param {string} text - The text to be spoken.
         * @returns {Promise<boolean>} - A promise that resolves to true on success, false on failure.
         */
        async speakText(text) {
            if (typeof text !== 'string' || text.trim() === '') {
                console.error(""NVDA_API Error: speakText requires a non-empty string."");
                return false;
            }
            const result = await _fetchAPI('/speak_text', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ text: text })
            });
            return result.success;
        },

        /**
         * Requests NVDA to cancel its current speech.
         * @returns {Promise<boolean>} - A promise that resolves to true on success, false on failure.
         */
        async cancelSpeech() {
            const result = await _fetchAPI('/cancel_speech', { method: 'POST' });
            return result.success;
        },

        /**
         * Requests NVDA to display a message on a connected braille display.
         * @param {string} text - The text to be displayed.
         * @returns {Promise<boolean>} - A promise that resolves to true on success, false on failure.
         */
        async showBrailleMessage(text) {
             if (typeof text !== 'string' || text.trim() === '') {
                console.error(""NVDA_API Error: showBrailleMessage requires a non-empty string."");
                return false;
            }
            const result = await _fetchAPI('/show_braille-message', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ text: text })
            });
            return result.success;
        }
    };

    window.NVDA_API = NVDA_API;
})(window);

";

		[Route(HttpVerbs.Get, "/nvda_api.js")]
		public async Task GetJsLibrary()
		{
			await this.HttpContext.SendStringAsync(NvdaWebApiJs, "application/javascript", Encoding.UTF8);
		}
	}
}
