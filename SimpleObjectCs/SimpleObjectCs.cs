using System;

using DataFSAccess;

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

            Connection pConnection = Connection.Create();

            if (0 > pConnection.Initialize(guidDomainId))
            {
                Connection.Destroy(pConnection);
                ThreadInit.UninitializeThread();
                return;
            }

            if (0 > pConnection.Connect(strServerAddress, usServerPort, null, null))
            {
                pConnection.Uninitialize();
                Connection.Destroy(pConnection);
                ThreadInit.UninitializeThread();
                return;
            }

            if (0 > pConnection.QueryStorage(ulStorageId, false, null))
            {
                pConnection.DisconnectAll();
                pConnection.Uninitialize();
                Connection.Destroy(pConnection);
                ThreadInit.UninitializeThread();
                return;
            }

            // build domain

            WDomain pWDomain = WDomain.Create();
            if (0 > pWDomain.Initialize(pConnection, 0))
            {
                WDomain.Destroy(pWDomain);
                pConnection.ReleaseAllStorages();
                pConnection.DisconnectAll();
                pConnection.Uninitialize();
                Connection.Destroy(pConnection);
                ThreadInit.UninitializeThread();
                return;
            }

            // bind types

            AccessDefinition.Bind(pWDomain);

            // open named object

            DataFS.Array<DataFS.ObjectId> aoiRootObject = new DataFS.Array<DataFS.ObjectId>();
            pWDomain.QueryNamedLinkId(new Guid[] { guidRootName }, aoiRootObject, null);

            TestRoot pRootObject;
            TestRoot.Open(out pRootObject, pWDomain, aoiRootObject.pData[0], 0, Transaction.Load);

            pRootObject.Load(_TestRoot.ALL_ATTRIBUTES, Transaction.Load);
            pWDomain.Execute(Transaction.Load, null);

            // open the list for writing

            TestObjectList pList;
            pRootObject.SetAllObjects(out pList);

            // create an add a new object

            TestObject pTestObject;
            TestObject.Create(out pTestObject, pRootObject.GetObject());

            TestObjectListItem itm = new TestObjectListItem();
            itm.anObject = pTestObject.BuildLink(true);
            itm.theType = 12;

            uint idx;
            pList.Insert(out idx, itm);

            pTestObject.SetText("something");
            pTestObject.SetNumber(343);

            pTestObject.StoreData(Transaction.Store);
            pRootObject.StoreData(Transaction.Store);

            pWDomain.Execute(Transaction.Store, null);

            pTestObject.Dispose();
            pRootObject.Dispose();

            // unbind types

            AccessDefinition.Unbind();

            // destroy domain

            pWDomain.Uninitialize();
            WDomain.Destroy(pWDomain);

            // disconnect

            pConnection.ReleaseAllStorages();
            pConnection.DisconnectAll();
            pConnection.Uninitialize();
            Connection.Destroy(pConnection);
            ThreadInit.UninitializeThread();
        }
    }
}
