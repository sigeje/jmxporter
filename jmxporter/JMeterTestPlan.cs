using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.IO;

namespace jmxporter
{
    public class JMeterTestPlan
    {
        private SessionList sessionList;
        private Fiddler.Session[] sessions;
        internal static Dictionary<string, object> args;

        public JMeterTestPlan()
        {
            sessions = new Fiddler.Session[0];
            sessionList = new SessionList(sessions);
        }

        public JMeterTestPlan(Fiddler.Session[] oSessions, string outputFilename)
        {
            this.sessions = oSessions;
            sessionList = new SessionList(oSessions);
        }

        public JMeterTestPlan(Fiddler.Session[] oSessions, string outputFilename, Dictionary<string, object> oArgs)
        {
            this.sessions = oSessions;
            sessionList = new SessionList(oSessions);
            args = oArgs;
        }

        public string Jmx 
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                XDocument doc = XDocument.Parse(this.Xml);
                sb.Append(doc.ToString());
                return sb.ToString();
            }
        }

        private string Xml 
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<jmeterTestPlan version=\"1.2\" properties=\"2.3\">");
                sb.Append(sessionList.Xml);
                sb.Append("</jmeterTestPlan>");
                return sb.ToString();
            }
        }
    }

    public class SessionList
    {
        private Fiddler.Session[] sessions;

        public SessionList()
        {
            sessions = new Fiddler.Session[0];
        }

        public SessionList(Fiddler.Session[] oSessions)
        {
            this.sessions = oSessions;
        }

        public string Xml 
        {
            get 
            {
                StringBuilder sb = new StringBuilder();
                if (sessions.Length > 0)
                {
                    sb.Append("<hashTree>");
                    foreach (Fiddler.Session session in sessions)
                    {
                        HTTPSamplerProxy httpSamplerProxy = new HTTPSamplerProxy(session);
                        sb.Append(httpSamplerProxy.Xml);
                    }
                    sb.Append("</hashTree>");
                }
                return sb.ToString();
            }
        }
    }

    public class HTTPSamplerProxy
    {
        Fiddler.Session session;

        public HTTPSamplerProxy(Fiddler.Session session)
        {
            this.session = session;
        }

        public string Xml
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                
                sb.Append(String.Format("<HTTPSamplerProxy guiclass=\"HttpTestSampleGui\" "
                    + "testclass=\"HTTPSamplerProxy\" testname=\"{0}\" enabled=\"true\">",
                    Path));
                
                if (JMeterTestPlan.args["useRaw"].Equals(true))
                {
                    sb.Append("<boolProp name=\"HTTPSampler.postBodyRaw\">true</boolProp>");
                }
                
                sb.Append("<elementProp name=\"HTTPsampler.Arguments\" elementType=\"Arguments\">");
                sb.Append("<collectionProp name=\"Arguments.arguments\">");

                if (JMeterTestPlan.args["useRaw"].Equals(true))
                {
                    sb.Append(RequestRawBody);
                }
                else
                {
                    sb.Append(RequestBody);
                }

                sb.Append("</collectionProp>");
                sb.Append("</elementProp>");
                sb.Append(String.Format("<stringProp name=\"HTTPSampler.domain\">{0}</stringProp>", session.host));
                sb.Append(String.Format("<stringProp name=\"HTTPSampler.port\">{0}</stringProp>", Port));
                sb.Append("<stringProp name=\"HTTPSampler.connect_timeout\"></stringProp>");
                sb.Append("<stringProp name=\"HTTPSampler.response_timeout\"></stringProp>");
                sb.Append(String.Format("<stringProp name=\"HTTPSampler.protocol\">{0}</stringProp>",
                    session.oRequest.headers.UriScheme));
                sb.Append("<stringProp name=\"HTTPSampler.contentEncoding\"></stringProp>");
                sb.Append(String.Format("<stringProp name=\"HTTPSampler.path\">{0}</stringProp>", Path));
                sb.Append(String.Format("<stringProp name=\"HTTPSampler.method\">{0}</stringProp>", 
                    session.oRequest.headers.HTTPMethod.ToUpper()));
                sb.Append("<boolProp name=\"HTTPSampler.follow_redirects\">true</boolProp>");
                sb.Append("<boolProp name=\"HTTPSampler.auto_redirects\">false</boolProp>");
                sb.Append("<boolProp name=\"HTTPSampler.use_keepalive\">true</boolProp>");
                sb.Append("<boolProp name=\"HTTPSampler.DO_MULTIPART_POST\">false</boolProp>");
                sb.Append("<boolProp name=\"HTTPSampler.monitor\">false</boolProp>");
                sb.Append("<stringProp name=\"HTTPSampler.embedded_url_re\"></stringProp>");
                sb.Append("</HTTPSamplerProxy>");
                sb.Append("<hashTree/>");
                return sb.ToString();
            }
        }

        private string Path
        {
            get
            {
                return System.Net.WebUtility.HtmlEncode(session.PathAndQuery);
            }
        }

        private string getPort()
        {
            int port = session.port;
            string protocol = session.oRequest.headers.UriScheme;
            if (protocol.ToLower() == ("https") && port == 443)
            {
                return "";
            }
            if (protocol.ToLower() == ("http") && port == 80)
            {
                return "";
            }
            return port.ToString();
        }

        private string Port
        {
            get
            {
                return getPort();
            }
        }

        private string RequestBody
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                string sRequestBody = session.GetRequestBodyAsString();
                foreach (var argument in sRequestBody.Split('&'))
                {
                    string[] strings = argument.Split('=');
                    string name = strings[0];
                    string value = strings.Length == 1 ? "" : strings[1];
                    sb.Append(String.Format("<elementProp name=\"{0}\" elementType=\"HTTPArgument\">", name));
                    sb.Append("<boolProp name=\"HTTPArgument.always_encode\">false</boolProp>");
                    sb.Append(String.Format("<stringProp name=\"Argument.name\">{0}</stringProp>", name));
                    sb.Append(String.Format("<stringProp name=\"Argument.value\">{0}</stringProp>", value));
                    sb.Append("<stringProp name=\"Argument.metadata\">=</stringProp>");
                    sb.Append("<boolProp name=\"HTTPArgument.use_equals\">true</boolProp>");
                    sb.Append("</elementProp>");
                }
                return sb.ToString();
            }
        }

        private string RequestRawBody
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<elementProp name=\"\" elementType=\"HTTPArgument\">");
                sb.Append("<boolProp name=\"HTTPArgument.always_encode\">false</boolProp>");
                sb.Append(String.Format("<stringProp name=\"Argument.value\">{0}</stringProp>", System.Net.WebUtility.HtmlEncode(session.GetRequestBodyAsString())));
                sb.Append("<stringProp name=\"Argument.metadata\">=</stringProp>");
                sb.Append("</elementProp>");
                return sb.ToString();
            }
        }
    }
}
