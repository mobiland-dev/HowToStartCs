using System;

using DataFSAccess;

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

			PrepareDefinition.Bind(pWDomain);

			// create named object

			TestRoot pRootObject;
			TestRoot.Create(out pRootObject, pWDomain, DataFS.ObjectId.OBJECTID_NULL);

			pRootObject.SetRootName("first test root");

			pRootObject.StoreData(Transaction.Store);

			pWDomain.InsertNamedLink(pRootObject.BuildLink(true), guidRootName, "first entry point", Transaction.Store);

			pWDomain.Execute(Transaction.Store, null);

			pRootObject.Dispose();

			// unbind types

			PrepareDefinition.Unbind();

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
