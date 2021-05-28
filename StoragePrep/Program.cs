using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataFoundationAccess;

namespace StoragePrep
{
    class Program
    {
        static readonly Guid guidRootName = new Guid("{56F8EB44-B7E3-4564-B9A6-22E5E1B9110C}");

        static void Main(string[] args)
        {
            String strServerAddress = args[0];
            UInt16 usServerPort = UInt16.Parse(args[1]);
            Guid guidDomainId = Guid.Parse(args[2]);

            UInt32 ulStorageId = 0;

            // connect
            DataFoundation.ThreadInit.InitializeThread();

            WDomain pWDomain = WDomain.New();
            if (0 > (pWDomain.Initialize()))
            {
                WDomain.Delete(pWDomain);
                DataFoundation.ThreadInit.UninitializeThread();
                return;
            }

            if (0 > (pWDomain.Connect(strServerAddress, usServerPort, guidDomainId, null)))
            {
                pWDomain.Uninitialize();
                WDomain.Delete(pWDomain);
                DataFoundation.ThreadInit.UninitializeThread();
                return;
            }

            if (0 > (pWDomain.QueryStorage(ulStorageId, false, null)))
            {
                pWDomain.DisconnectAll();
                pWDomain.Uninitialize();
                WDomain.Delete(pWDomain);
                DataFoundation.ThreadInit.UninitializeThread();
                return;
            }

            // bind types
            PrepareDefinition.Bind(pWDomain);

            // create named object

            ITestRoot pRootObject;

            PrepareDefinition.Create(pWDomain, out pRootObject);

            pRootObject.SetRootName("first test root");

            pRootObject.StoreData(WDomain.TRANSACTION_STORE);

            pWDomain.InsertNamedObject(pRootObject.BuildLink(true), guidRootName, "first entry point", WDomain.TRANSACTION_STORE);

            pWDomain.Execute(WDomain.TRANSACTION_STORE);

            pRootObject.Release();

            // unbind types
            PrepareDefinition.Unbind();

            // disconnect

            pWDomain.ReleaseStorage(ulStorageId);
            pWDomain.DisconnectAll();
            pWDomain.Uninitialize();
            WDomain.Delete(pWDomain);
            DataFoundation.ThreadInit.UninitializeThread();
        }
    }
}
