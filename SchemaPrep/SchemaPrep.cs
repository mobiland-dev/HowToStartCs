using System;
using System.IO;

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

            DataFS.ThreadInit.InitializeThread();

            DataFS.Connection pConnection = DataFS.Connection.Create();

            if (0 > (pConnection.Connect(strServerAddress, usServerPort, null)))
            {
                DataFS.Connection.Destroy(pConnection);
                DataFS.ThreadInit.UninitializeThread();
                return;
            }

            // extend schema

            Stream sBdtd = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("DataDefinition");

            Byte[] pBdtd = new Byte[sBdtd.Length];
            sBdtd.Read(pBdtd, 0, (int)sBdtd.Length);

            DataFS.USchemaEdit pSchema;

            if (0 <= (pConnection.QuerySchemaEdit(out pSchema, guidDomainId)))
            {
                pSchema.CreateFromBinary(pBdtd);
                pSchema.Commit();
                pSchema.Dispose();
            }

            pConnection.Disconnect();
            DataFS.Connection.Destroy(pConnection);
            DataFS.ThreadInit.UninitializeThread();
        }
    }
}
