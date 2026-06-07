using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using MongoDB.Driver;

namespace DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly IMongoCollection<DocumentBase> _collection;

        public DocumentRepository(IMongoClient mongoClient)
        {
            // Aspire inietta automaticamente il MongoClient. Recupero il database e la collection.
            var database = mongoClient.GetDatabase("documentdb");
            _collection = database.GetCollection<DocumentBase>("Documents");
        }

        public async Task<T?> GetByIdAsync<T>(string id) where T : DocumentBase
        {
            var filter = Builders<DocumentBase>.Filter.Eq(doc => doc.Id, id);

            var document = await _collection
                                    .Find(filter)
                                    .FirstOrDefaultAsync();

            // Cast al tipo specifico richiesto (Quote, Proforma, SalesOrder)
            return document as T;
        }

        public async Task CreateAsync(DocumentBase document)
        {
            // Valori di base per un nuovo inserimento
            document.Version = 1;
            document.CreatedAt = DateTime.UtcNow;

            await _collection.InsertOneAsync(document);
        }

        public async Task UpdateAsync(DocumentBase document)
        {
            document.UpdatedAt = DateTime.UtcNow;

            // Versione attuale che ci aspettiamo di trovare sul database
            var expectedVersion = document.Version;

            // Incremento la versione sull'oggetto in memoria che stiamo per salvare
            document.Version++;

            // Cerco il documento con questo specifico Id con la vecchia versione
            var filter = Builders<DocumentBase>.Filter.And(
                Builders<DocumentBase>.Filter.Eq(doc => doc.Id, document.Id),
                Builders<DocumentBase>.Filter.Eq(doc => doc.Version, expectedVersion)
            );

            // Uso ReplaceOneAsync per sostituire l'intero documento.
            var result = await _collection.ReplaceOneAsync(filter, document);

            // Se ModifiedCount è 0, significa che il documento è stato eliminato oppure 
            // la versione sul DB era diversa (qualcun altro ha modificato prima).
            if (result.ModifiedCount == 0)
            {
                // Rollback della versione in memoria in caso l'eccezione venga gestita per un retry
                document.Version = expectedVersion;

                //TODO: Per ora lancio un'eccezione standard. Più avanti bisognerà creare una 'ConcurrencyException' custom nei BuildingBlocks.
                throw new InvalidOperationException($"Concorrenza rilevata: impossibile aggiornare il documento {document.Id}. È stato modificato da un altro utente.");
            }
        }

        public async Task<bool> UpdateStatusWithConcurrencyAsync(string id, DocumentStatus newStatus, int expectedVersion)
        {
            // Criterio di ricerca: trovo il documento per Id solo se la versione corrisponde a quella attesa
            var filter = Builders<DocumentBase>.Filter.And(
                Builders<DocumentBase>.Filter.Eq(doc => doc.Id, id),
                Builders<DocumentBase>.Filter.Eq(doc => doc.Version, expectedVersion)
            );

            // Definizione degli aggiornamenti: setto il nuovo stato, aggiorniamo la data e incrementiamo la versione
            var update = Builders<DocumentBase>.Update
                .Set(doc => doc.Status, newStatus)
                .Set(doc => doc.UpdatedAt, DateTime.UtcNow)
                .Inc(doc => doc.Version, 1);

            var result = await _collection.UpdateOneAsync(filter, update);

            // Se ModifiedCount è 0, significa che la versione non corrispondeva (concorrenza) o il documento non esiste
            return result.ModifiedCount > 0;
        }
    }
}
