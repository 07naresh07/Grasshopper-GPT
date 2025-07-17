using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Grasshopper_GPT.UtilComps
{
    public class HTTPPostRequestComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the HTTPPostRequestComponent class.
        /// </summary>
        public HTTPPostRequestComponent()
          : base("HTTPPostRequestComponent", "POST",
              "Creates a generic HTTP POST request (synchronous)",
              "GPT", "Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // active
            pManager.AddBooleanParameter("Send", "S", "Perform the request", GH_ParamAccess.item, false);
            // url (endpoint)
            pManager.AddTextParameter("URL", "U", "URL for the request", GH_ParamAccess.item);
            // body
            pManager.AddTextParameter("Body", "B", "Body of the request", GH_ParamAccess.item);
            // content/type
            pManager.AddTextParameter("Content Type", "T", "Content type for the request, such as \"application/json\", \"text/html\", etc.", GH_ParamAccess.item, "application/json");
            // custom headers
            // authentication
            int authId = pManager.AddTextParameter("Authorization", "A", "If this request requires authorization, input your formatted token as an Auth string, e.g. \"Bearer h1g23g1fdg3d1\"", GH_ParamAccess.item);
            // timeout
            pManager.AddIntegerParameter("Timeout", "T", "Timeout for the request in ms. If the request takes longer than this, it will fail.", GH_ParamAccess.item, 10000);

            pManager[authId].Optional = true;   // Make authentication optional (e.g. for public APIs)
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Response", "R", "Request response from server", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool active = false;
            string url = "";
            string body = "";
            string contentType = "";
            string authToken = "";
            int timeout = 0;

            // Retrieve input parameters
            DA.GetData("Send", ref active);
            if (!active)
            {
                DA.SetData("Response", "");
                return; // If not active, do nothing
            }
            if (!DA.GetData("URL", ref url)) return;
            if (!DA.GetData("Body", ref body)) return;
            if (!DA.GetData("Content Type", ref contentType)) return;
            DA.GetData("Authorization", ref authToken);
            if (!DA.GetData("Timeout", ref timeout)) return;

            // Validity checks
            if (url == null || url.Length == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Empty URL");
                return;
            }

            // Check if content type is null or empty
            if (contentType == null || contentType.Length == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Empty Content Type");
                return;
            }
            // Compose the request
            byte[] data = Encoding.ASCII.GetBytes(body);    // Convert body to byte array i.e. 0 and 1s
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";  // Set the method to POST
            request.ContentType = contentType;  // Set the content type
            request.ContentLength = data.Length;  // Set the content length
            request.Timeout = timeout;  // Set the timeout

            // Handle authorization
            if (authToken != null && authToken.Length > 0)
            {
                System.Net.ServicePointManager.Expect100Continue = true;
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12; // Ensure TLS 1.2 is used
                request.PreAuthenticate = true;
                request.Headers.Add("Authorization", authToken);
            }
            else
            {
                request.Credentials = CredentialCache.DefaultCredentials;
            }
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);  // Write the body data to the request stream
            }
            string response = "";
            try
            {
                var res = request.GetResponse();  // Get the response from the server
                var reader = new StreamReader(res.GetResponseStream());
                response = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Something went wrong: " + ex.Message);
                return; // If an error occurs, exit the method
            }
            // Set the response output
            DA.SetData(0, response);
        }
            /// <summary>
            /// Provides an Icon for the component.
            /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E5F78476-908E-4149-83FA-CF90265874B1"); }
        }
    }
}