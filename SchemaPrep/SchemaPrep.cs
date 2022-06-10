using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchemaPrep
{
    class SchemaPrep
    {
        static void Main(string[] args)
        {
            String strServerAddress = args[0];
            UInt16 usServerPort = UInt16.Parse(args[1]);
            Guid guidDomainId = Guid.Parse(args[2]);

            // connect

            DataFoundation.ThreadInit.InitializeThread();

            DataFoundation.Connection pConnection = DataFoundation.Connection.Create();

            if (0 > (pConnection.Connect(strServerAddress, usServerPort, null)))
            {
                DataFoundation.Connection.Destroy(pConnection);
                DataFoundation.ThreadInit.UninitializeThread();
                return;
            }

            // extend schema

            Stream sBdtd = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("DataDefinition");

            Byte[] pBdtd = new Byte[sBdtd.Length];
            sBdtd.Read(pBdtd, 0, (int)sBdtd.Length);

            DataFoundation.USchemaEdit pSchema;

            if (0 <= (pConnection.QuerySchemaEdit(out pSchema, guidDomainId)))
            {
                pSchema.CreateFromBinary(pBdtd);
                pSchema.Commit();
                pSchema.Dispose();
            }

            pConnection.Disconnect();
            DataFoundation.Connection.Destroy(pConnection);
            DataFoundation.ThreadInit.UninitializeThread();
        }
    }
}
