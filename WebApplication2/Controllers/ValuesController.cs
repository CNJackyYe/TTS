using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

namespace WebApplication2.Controllers
{
    [RoutePrefix("api/TTS")]
    public class TTSController : ApiController
    {

        [HttpGet, Route("GetPath")]
        public HttpResponseMessage GetPath(string Text, int rate = -2)
        {
            try
            {
                string phrase = Text;
                var name = Guid.NewGuid().ToString("N");
                var path = HostingEnvironment.MapPath(@"~\mp3\");
                var truepath = path + name + ".wav";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var t = new System.Threading.Thread(() =>
                {
                    using (var synth = new SpeechSynthesizer())
                    {
                        synth.Rate = rate;
                        synth.Volume = 100;
                        synth.SetOutputToWaveFile(truepath, new SpeechAudioFormatInfo(8000, AudioBitsPerSample.Eight, AudioChannel.Mono));
                        synth.Speak(phrase);
                        synth.SetOutputToNull();
                    }
                });

                t.Start();
                t.Join();

                var dataBytes = File.ReadAllBytes(truepath);
                var dataStream = new MemoryStream(dataBytes);
                var resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StreamContent(dataStream);

                resp.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");
                resp.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                resp.Content.Headers.ContentDisposition.FileName = name + ".wav";
                File.Delete(truepath);
                return resp;
            }
            catch
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

    }
}
