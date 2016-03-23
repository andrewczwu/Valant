using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using ValantInv.Hubs;
using ValantInv.Models;

namespace ValantInv.Controllers
{
    [InvalidModelStateFilter]
    public class ItemController : ApiControllerWithHub<ToDoHub>
    {
        private static List<InventoryItem> db = new List<InventoryItem>
        {
            new InventoryItem { ID = 0, Label = "Spread the word about ASP.NET Web API" },
            new InventoryItem { ID = 1, Label = "Wash the car" },
            new InventoryItem { ID = 2, Label = "Get a haircut" }
        };
        private static int lastId = db.Max(tdi => tdi.ID);

        public IEnumerable<InventoryItem> GetToDoItems()
        {
            lock (db)
                return db.ToArray();
        }

        public InventoryItem GetToDoItem(int id)
        {
            lock (db)
            {
                var item = db.SingleOrDefault(i => i.ID == id);
                if (item == null)
                    throw new HttpResponseException(
                        Request.CreateResponse(HttpStatusCode.NotFound)
                    );

                return item;
            }
        }

        public HttpResponseMessage PostNewToDoItem(InventoryItem item)
        {
            lock (db)
            {
                // Add item to the "database"
                item.ID = Interlocked.Increment(ref lastId);
                db.Add(item);

                // Notify the connected clients
                Hub.Clients.addItem(item);

                // Return the new item, inside a 201 response
                var response = Request.CreateResponse(HttpStatusCode.Created, item);
                string link = Url.Link("apiRoute", new { controller = "todo", id = item.ID });
                response.Headers.Location = new Uri(link);
                return response;
            }
        }

        public InventoryItem PutUpdatedToDoItem(int id, InventoryItem item)
        {
            lock (db)
            {
                // Find the existing item
                var toUpdate = db.SingleOrDefault(i => i.ID == id);
                if (toUpdate == null)
                    throw new HttpResponseException(
                        Request.CreateResponse(HttpStatusCode.NotFound)
                    );

                // Update the editable fields and save back to the "database"


                // Notify the connected clients
                Hub.Clients.updateItem(toUpdate);

                // Return the updated item
                return toUpdate;
            }
        }

        public HttpResponseMessage DeleteToDoItem(int id)
        {
            lock (db)
            {
                int removeCount = db.RemoveAll(i => i.ID == id);
                if (removeCount <= 0)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                // Notify the connected clients
                Hub.Clients.deleteItem(id);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
}
