using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataFoundationAccess;

namespace SimpleObjectCs
{
    class SimpleObject
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

            AccessDefinition.Bind(pWDomain);

            // open named object

            DataFoundation.ObjectId[] aoiRootObject;
            pWDomain.QueryNamedObjectId(new Guid[] { guidRootName }, out aoiRootObject);

            ITestRoot pRootObject;
            ITestRoot.Open(out pRootObject, aoiRootObject[0], pWDomain, Transaction.Load);

            pRootObject.Load(_ITestRoot.ALL_ATTRIBUTES, Transaction.Load);
            pWDomain.Execute(Transaction.Load);

            // open the list for writing

            TestObjectList pList;
            pRootObject.SetAllObjects(out pList);

            // create an add a new object

            ITestObject pTestObject;
            ITestObject.Create(out pTestObject, pRootObject);

            TestObjectListItem itm = new TestObjectListItem();
            itm.anObject = pTestObject.BuildLink(true);
            itm.theType = 12;

            uint idx;
            pList.Insert(out idx, itm);

            pTestObject.SetText("something");
            pTestObject.SetNumber(343);

            pTestObject.StoreData(Transaction.Store);
            pRootObject.StoreData(Transaction.Store);

            pWDomain.Execute(Transaction.Store);

            pTestObject.Dispose();
            pRootObject.Dispose();

            // unbind types

            AccessDefinition.Unbind();

            // disconnect

            pWDomain.ReleaseStorage(ulStorageId);
            pWDomain.DisconnectAll();
            pWDomain.Uninitialize();
            WDomain.Destroy(pWDomain);
            ThreadInit.UninitializeThread();
        }
    }
}
