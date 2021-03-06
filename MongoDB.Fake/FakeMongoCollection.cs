﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Fake.Filters.Parsers;

namespace MongoDB.Fake
{
    public class FakeMongoCollection<TDocument> : SyncMongoCollection<TDocument>
    {
        private readonly IFilterParser _filterParser;
        private readonly BsonDocumentCollection _documents;

        private readonly FakeMongoDatabase _database;

        public override CollectionNamespace CollectionNamespace
        {
            get { throw new NotImplementedException(); }
        }

        public override IMongoDatabase Database => _database;

        public override IBsonSerializer<TDocument> DocumentSerializer
        {
            get { throw new NotImplementedException(); }
        }

        public override IMongoIndexManager<TDocument> Indexes
        {
            get { throw new NotImplementedException(); }
        }

        public override MongoCollectionSettings Settings
        {
            get { throw new NotImplementedException(); }
        }

        public FakeMongoCollection()
            : this(new BsonDocumentCollection())
        {
        }

        public FakeMongoCollection(BsonDocumentCollection documents)
        {
            _documents = documents;

            _filterParser = FilterParser.Instance;
        }

        internal FakeMongoCollection(FakeMongoDatabase database, BsonDocumentCollection documents)
            : this(documents)
        {
            _database = database;
        }

        public override IAsyncCursor<TProjection> FindSync<TProjection>(FilterDefinition<TDocument> filter, FindOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var filteredDocuments = Filter(filter);
            var projectedDocuments = Project(filteredDocuments, options?.Projection);
            return new AsyncCursor<TProjection>(projectedDocuments);
        }

        public override TProjection FindOneAndDelete<TProjection>(FilterDefinition<TDocument> filter, FindOneAndDeleteOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var documentToDelete = Filter(filter).FirstOrDefault();
            if (documentToDelete == null)
            {
                return default(TProjection);
            }
            _documents.Remove(documentToDelete);
            var projectedDocument = Project(new[] { documentToDelete }, options?.Projection).Single();
            return projectedDocument;
        }

        public override TProjection FindOneAndReplace<TProjection>(FilterDefinition<TDocument> filter, TDocument replacement, FindOneAndReplaceOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var documentToDelete = Filter(filter).FirstOrDefault();
            if (documentToDelete == null)
            {
                return default(TProjection);
            }

            _documents.Remove(documentToDelete);

            var documentToAdd = SerializeDocument(replacement);
            _documents.Add(documentToAdd);

            var projectedDocument = Project(new[] { documentToDelete }, options?.Projection).Single();
            return projectedDocument;
        }

        public override TProjection FindOneAndUpdate<TProjection>(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, FindOneAndUpdateOptions<TDocument, TProjection> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public override void InsertOne(TDocument document, InsertOneOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var bsonDocument = SerializeDocument(document);
            _documents.Add(bsonDocument);
        }

        public override void InsertMany(IEnumerable<TDocument> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var document in documents)
            {
                var bsonDocument = SerializeDocument(document);
                _documents.Add(bsonDocument);
            }
        }

        public override BulkWriteResult<TDocument> BulkWrite(IEnumerable<WriteModel<TDocument>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public override UpdateResult UpdateOne(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public override UpdateResult UpdateMany(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public override ReplaceOneResult ReplaceOne(FilterDefinition<TDocument> filter, TDocument replacement, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public override DeleteResult DeleteMany(FilterDefinition<TDocument> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteMany(filter, null, cancellationToken);
        }

        public override DeleteResult DeleteMany(FilterDefinition<TDocument> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var documentsToDelete = Filter(filter).ToList();
            foreach (var documentToDelete in documentsToDelete)
            {
                _documents.Remove(documentToDelete);
            }

            return new DeleteResult.Acknowledged(documentsToDelete.Count);
        }

        public override DeleteResult DeleteOne(FilterDefinition<TDocument> filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DeleteOne(filter, null, cancellationToken);
        }

        public override DeleteResult DeleteOne(FilterDefinition<TDocument> filter, DeleteOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var documentToDelete = Filter(filter).FirstOrDefault();
            if (documentToDelete == null)
            {
                return new DeleteResult.Acknowledged(0);
            }

            _documents.Remove(documentToDelete);
            return new DeleteResult.Acknowledged(1);
        }

        public override IAsyncCursor<TResult> Aggregate<TResult>(PipelineDefinition<TDocument, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public override long Count(FilterDefinition<TDocument> filter, CountOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var documents = Filter(filter);
            return documents.LongCount();
        }

        public override IAsyncCursor<TField> Distinct<TField>(FieldDefinition<TDocument, TField> field, FilterDefinition<TDocument> filter, DistinctOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public override IAsyncCursor<TResult> MapReduce<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<TDocument, TResult> options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public override IFilteredMongoCollection<TDerivedDocument> OfType<TDerivedDocument>()
        {
            throw new NotImplementedException();
        }

        public override IMongoCollection<TDocument> WithReadConcern(ReadConcern readConcern)
        {
            throw new NotImplementedException();
        }

        public override IMongoCollection<TDocument> WithReadPreference(ReadPreference readPreference)
        {
            throw new NotImplementedException();
        }

        public override IMongoCollection<TDocument> WithWriteConcern(WriteConcern writeConcern)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<BsonDocument> Filter(FilterDefinition<TDocument> filter)
        {
            var documentSerializer = BsonSerializer.SerializerRegistry.GetSerializer<TDocument>();
            var filterBson = filter.Render(documentSerializer, BsonSerializer.SerializerRegistry);

            var parsedFilter = _filterParser.Parse(filterBson);

            return _documents.Where(parsedFilter.Filter);
        }

        private IEnumerable<TProjection> Project<TProjection>(IEnumerable<BsonDocument> documents, ProjectionDefinition<TDocument, TProjection> projection)
        {
            IBsonSerializer<TProjection> serializer;

            if (projection != null)
            {
                throw new NotImplementedException();
            }
            else
            {
                serializer = BsonSerializer.LookupSerializer<TProjection>();
            }

            foreach (var document in documents)
            {
                yield return DeserializeDocument(document, serializer);
            }
        }

        private BsonDocument SerializeDocument<T>(T document)
        {
            var serializer = BsonSerializer.LookupSerializer<T>();
            return SerializeDocument(document, serializer);
        }

        private BsonDocument SerializeDocument<T>(T document, IBsonSerializer<T> serializer)
        {
            return document.ToBsonDocument(serializer);
        }

        private T DeserializeDocument<T>(BsonDocument document, IBsonSerializer<T> serializer)
        {
            using (var reader = new BsonDocumentReader(document))
            {
                var deserializationContext = BsonDeserializationContext.CreateRoot(reader);
                return serializer.Deserialize(deserializationContext);
            }
        }
    }
}
