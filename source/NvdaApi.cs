using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NVDASpeechWebApi
{
	static class NvdaApi
	{
		const string DllName = @"nvdaControllerClient.dll";

		[DllImport(DllName, CharSet = CharSet.Unicode)]
		private static extern int nvdaController_brailleMessage(string message);

		[DllImport(DllName)]
		private static extern int nvdaController_cancelSpeech();

		[DllImport(DllName, CharSet = CharSet.Unicode)]
		private static extern int nvdaController_speakText(string text);

		[DllImport(DllName)]
		private static extern int nvdaController_testIfRunning();

		[DllImport(DllName)]
		private static extern int nvdaController_getProcessId(out uint processId);

		public static uint GetNvdaPid()
		{
			return (nvdaController_getProcessId(out uint pid) > 0 ?0:pid);
		}

		public static int CancelSpeech()
		{
			return (nvdaController_cancelSpeech());
		}

		public static int NvdaSpeakText(string text)
		{
			return (nvdaController_speakText(text));
		}

		public static int ShowBrailleMessage(string text)
		{
			return (nvdaController_brailleMessage(text));
		}
	}
}
