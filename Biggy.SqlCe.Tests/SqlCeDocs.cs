﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biggy;
using Biggy.SQLServer;
using Xunit;
//using Newtonsoft.Json;

namespace Biggy.SqlCe.Tests {

  [Trait("SQL Server Document Store","")]
  public class SqlCeDocs {

    public string _connectionStringName = "chinook";

    SqlCeDocumentStore<ClientDocument> clientDocs;
    IQueryable<ClientDocument> qClientDocs;
    public SqlCeDocs() {
      // Start fresh each time, with no existing table, to keep serial PK's from exploding:
      var th = new TableHelpers(_connectionStringName);
      if(th.TableExists("ClientDocuments")) {
        th.DropTable("ClientDocuments");
      }

      clientDocs = new SqlCeDocumentStore<ClientDocument>(_connectionStringName);
      qClientDocs = (clientDocs as IQueryableBiggyStore<ClientDocument>).AsQueryable();
      // clientDocs.DeleteAll();
    }

    [Fact(DisplayName = "SqlCe store should implement IQueryableBiggyStore")]
    public void StoreShouldBeQuerable() {
        Assert.NotNull(qClientDocs);
    }

    [Fact(DisplayName = "Creates a store with a serial PK if one doesn't exist")]
    public void Creates_Document_Table_With_Serial_PK_If_Not_Present() {
      Assert.True(qClientDocs.Count() == 0);
    }


    [Fact(DisplayName = "Adds a document with a serial PK")]
    public void Adds_Document_With_Serial_PK() {
      var newCustomer = new ClientDocument { 
        Email = "rob@tekpub.com", 
        FirstName = "Rob", 
        LastName = "Conery" };
      clientDocs.Insert(newCustomer);
      Assert.Equal(1, qClientDocs.Count());
    }

    [Fact(DisplayName = "Newly added document with a serial PK have Pk property set to autogenerated value inside body")]
    public void New_Document_With_Serial_PK_Have_Id_Propoerty_set() {
        var newCustomer = new ClientDocument {
            Email = "rob@tekpub.com",
            FirstName = "Rob",
            LastName = "Conery"
        };
        clientDocs.Insert(newCustomer);
        int idToFind = newCustomer.ClientDocumentId;

        //clientDocs.Reload();
        var document = qClientDocs.FirstOrDefault(cd => cd.ClientDocumentId == idToFind);
        Assert.NotNull(document);
    }

    [Fact(DisplayName = "Updates a document with a serial PK")]
    public void Updates_Document_With_Serial_PK() {
      var newCustomer = new ClientDocument {
        Email = "rob@tekpub.com",
        FirstName = "Rob",
        LastName = "Conery"
      };
      clientDocs.Insert(newCustomer);
      int idToFind = newCustomer.ClientDocumentId;
      // Go find the new record after reloading:
      //clientDocs.Reload();
      var updateMe = qClientDocs.FirstOrDefault(cd => cd.ClientDocumentId == idToFind);
      // Update:
      updateMe.FirstName = "Bill";
      clientDocs.Update(updateMe);
      // Go find the updated record after reloading:
      //clientDocs.Reload();
      var updated = qClientDocs.FirstOrDefault(cd => cd.ClientDocumentId == idToFind);
      Assert.True(updated.FirstName == "Bill");
    }


    [Fact(DisplayName = "Deletes a document with a serial PK")]
    public void Deletes_Document_With_Serial_PK() {
      var newCustomer = new ClientDocument {
        Email = "rob@tekpub.com",
        FirstName = "Rob",
        LastName = "Conery"
      };
      clientDocs.Insert(newCustomer);
      // Count after adding new:
      int initialCount = qClientDocs.Count();
      var removed = clientDocs.Delete(newCustomer);
      //clientDocs.Reload();
      // Count after removing and reloading:
      int finalCount = qClientDocs.Count();
      Assert.NotNull(removed);
      Assert.True(finalCount < initialCount);
    }


    [Fact(DisplayName = "Bulk-Inserts new records as JSON documents with serial int key")]
    static void Bulk_Inserts_Documents_With_Serial_PK() {
      int insertQty = 100;
      var ClientDocuments = new SqlCeDocumentStore<ClientDocument>("chinook");
      var bulkList = new List<ClientDocument>();
      for (int i = 0; i < insertQty; i++) {
        var newClientDocument = new ClientDocument { 
          FirstName = "ClientDocument " + i, 
          LastName = "Test",
          Email = "jatten@example.com"
        };
        bulkList.Add(newClientDocument);
      }
      var inserted = ClientDocuments.BulkInsert(bulkList);

      var last = inserted.Last();
      Assert.True(inserted.Count == insertQty && last.ClientDocumentId >= insertQty);
    }


  }
}
