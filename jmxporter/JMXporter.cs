using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Fiddler;
// Fiddler minimum version required
[assembly: Fiddler.RequiredVersion("4.4.9.3")]

namespace jmxporter
{
    [ProfferFormat("JMeter", "JMeter .jmx Format")]
    public class JMXporter : ISessionExporter
    {
        public bool ExportSessions(string sFormat, Session[] oSessions, Dictionary<string, object> dictOptions,
            EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
        {
            bool bResult = true;
            string sFilename = null;

            sFilename = Fiddler.Utilities.ObtainSaveFilename("Export As " + sFormat, "JMeter Files (*.jmx)|*.jmx");

            if (String.IsNullOrEmpty(sFilename)) return false;

            if (!Path.HasExtension(sFilename)) sFilename = sFilename + ".jmx";

            try
            {
                Encoding encUTF8NoBOM = new UTF8Encoding(false);

                JMeterTestPlan jMeterTestPlan = new JMeterTestPlan(oSessions, sFilename);
                System.IO.StreamWriter sw = new StreamWriter(sFilename, false, encUTF8NoBOM);

                sw.Write(jMeterTestPlan.Jmx);
                sw.Close();

                Fiddler.FiddlerApplication.Log.LogString("Successfully exported sessions to JMeter Test Plan");
                Fiddler.FiddlerApplication.Log.LogString(String.Format("\t{0}", sFilename));
            }
            catch (Exception eX)
            {
                Fiddler.FiddlerApplication.Log.LogString(eX.Message);
                Fiddler.FiddlerApplication.Log.LogString(eX.StackTrace);
                bResult = false;
            }

            return bResult;
        }

        public void Dispose()
        {
        }
    }
}
