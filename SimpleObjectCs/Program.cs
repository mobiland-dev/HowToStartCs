using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataFoundationAccess;

namespace SimpleObjectCs
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
            AccessDefinition.Bind(pWDomain);

            // open named object

            DataFoundation.ObjectId oiRootObject;

            pWDomain.QueryNamedObjectId(guidRootName, out oiRootObject);

            ITestRoot pRootObject;
            AccessDefinition.Open(pWDomain, out pRootObject, oiRootObject, WDomain.TRANSACTION_LOAD);

            pRootObject.Load(_ITestRoot.ALL_ATTRIBUTES, WDomain.TRANSACTION_LOAD);
            pWDomain.Execute(WDomain.TRANSACTION_LOAD);

            // open the list for writing

            TestObjectList pList;
            pRootObject.SetAllObjects(out pList);

            // create an add a new object

            ITestObject pTestObject;

            pRootObject.Create(out pTestObject);

            TestObjectListItem itm = new TestObjectListItem();
            itm.anObject = pTestObject.BuildLink(true);
            itm.theType = 12;

            uint idx;
            pList.Insert(out idx, itm);

            pTestObject.SetText("something");
            pTestObject.SetNumber(343);

            pTestObject.StoreData(WDomain.TRANSACTION_STORE);
            pRootObject.StoreData(WDomain.TRANSACTION_STORE);

            pWDomain.Execute(WDomain.TRANSACTION_STORE);

            pTestObject.Release();
            pRootObject.Release();

            // unbind types
            AccessDefinition.Unbind();

            // disconnect

            pWDomain.ReleaseStorage(ulStorageId);
            pWDomain.DisconnectAll();
            pWDomain.Uninitialize();
            WDomain.Delete(pWDomain);
            DataFoundation.ThreadInit.UninitializeThread();
        }
    }
}
