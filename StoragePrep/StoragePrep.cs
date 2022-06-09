using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataFoundationAccess;

namespace StoragePrep
{
    class StoragePrep
    {
        static readonly Guid guidRootName = new Guid("{56F8EB44-B7E3-4564-B9A6-22E5E1B9110C}");

        static void Main(string[] args)
        {
            String strServerAddress = args[0];
            UInt16 usServerPort = UInt16.Parse(args[1]);
            Guid guidDomainId = Guid.Parse(args[2]);

            UInt32 ulStorageId = 0;

            // connect

            ThreadInit.InitializeThread();

            WDomain pWDomain = WDomain.Create();
            if (0 > (pWDomain.Initialize(guidDomainId)))
            {
                WDomain.Destroy(pWDomain);
                ThreadInit.UninitializeThread();
                return;
            }

            if (0 > (pWDomain.Connect(strServerAddress, usServerPort, null)))
            {
                pWDomain.Uninitialize();
                WDomain.Destroy(pWDomain);
                ThreadInit.UninitializeThread();
                return;
            }

            if (0 > (pWDomain.QueryStorage(ulStorageId, false, null)))
            {
                pWDomain.DisconnectAll();
                pWDomain.Uninitialize();
                WDomain.Destroy(pWDomain);
                ThreadInit.UninitializeThread();
                return;
            }

            // bind types
            PrepareDefinition.Bind(pWDomain);

            // create named object

            ITestRoot pRootObject;
            ITestRoot.Create(out pRootObject, pWDomain);

            pRootObject.SetRootName("first test root");

            pRootObject.StoreData(Transaction.Store);

            pWDomain.InsertNamedObject(pRootObject.BuildLink(true), guidRootName, "first entry point", Transaction.Store);

            pWDomain.Execute(Transaction.Store);

            pRootObject.Dispose();

            // unbind types
            PrepareDefinition.Unbind();

            // disconnect

            pWDomain.ReleaseStorage(ulStorageId);
            pWDomain.DisconnectAll();
            pWDomain.Uninitialize();
            WDomain.Destroy(pWDomain);
            ThreadInit.UninitializeThread();
        }
    }
}
