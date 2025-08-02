using Elasticsearch.Net;
using Library.Data;
using Library.Models;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace Library.Services;
public class BookIndexService
{
    private readonly ElasticClient _client;
    private readonly AppDbContext _appDbContext;
    private const string IndexName = "books";

    public BookIndexService(ElasticClient client,AppDbContext appDbContext)
    {
        _client = client;
        _appDbContext = appDbContext;
    }

    public async Task InitializeAsync()
    {
        var pingResponse = await _client.PingAsync();
        if (pingResponse.IsValid)
        {
            Console.WriteLine("===> Connected to Elasticsearch!");
        }
        else
        {
            Console.WriteLine("===> Not connected to Elasticsearch!");
            Console.WriteLine($"Error: {pingResponse.OriginalException?.Message}");
            return;
        }

        var indexExists = await _client.Indices.ExistsAsync(IndexName);
        if (!indexExists.Exists)
        {
            Console.WriteLine($"📁 Index '{IndexName}' not found. Creating...");
            var createIndexResponse = await _client.Indices.CreateAsync(IndexName, c => c
                .Map<Book>(m => m
                    .AutoMap()
                    .Properties(p => p
                        .Text(t => t.Name(n => n.Title))
                        .Text(t => t.Name(n => n.Description))
                    )
                )
            );

            if (createIndexResponse.IsValid)
                Console.WriteLine("===> Index created successfully.");

            else
                Console.WriteLine($"===> Index creation failed: {createIndexResponse.ServerError?.Error.Reason}");
        }
        else
        {
            Console.WriteLine($"===> Index '{IndexName}' already exists.");
        }
    }

    public async Task IndexBookAsync(Book book)
    {
        var dto = new IndexBookDto
        {
            Id = book.Id,
            Title = book.Title,
            Description = book.Description,
            AuthorId = book.AuthorId,
            CategoryId = book.CategoryId
        };

        var indexResponse = await _client.IndexDocumentAsync(dto);

        if (!indexResponse.IsValid)
            Console.WriteLine($"===> Indexing error: {indexResponse.ServerError?.Error.Reason}");
        else
            Console.WriteLine($"===> Book indexed to Elasticsearch (Id: {dto.Id})");

    }

    public async Task DeleteBookAsync(int bookId)
    {
        var deleteResponse = await _client.DeleteAsync<IndexBookDto>(bookId, d => d.Index(IndexName));

        if (deleteResponse.IsValid)
            Console.WriteLine($"===> Book deleted from Elasticsearch index (Id: {bookId})");

        else
            Console.WriteLine($"===> Deletion error: {deleteResponse.ServerError?.Error.Reason}");

    }

    public async Task<IEnumerable<Book>> SearchAsync(string query)
    {
        var response = await _client.SearchAsync<Book>(s => s
            .Query(q => q
                .MultiMatch(m => m
                    .Fields(f => f
                        .Field(b => b.Title)
                        .Field(b => b.Description)
                    )
                    .Query(query)
                )
            )
        );

        //Console.WriteLine("IsValid: " + response.IsValid);
        //Console.WriteLine("ServerError: " + (response.ServerError != null ? response.ServerError.Error.Reason : "No error"));
        //Console.WriteLine("Debug info: " + response.DebugInformation);

        if (!response.IsValid)
        {
            return Enumerable.Empty<Book>();
        }
       
        var books = response.Documents.ToList();

        foreach (var book in books)
        {
            book.Author = await _appDbContext.Authors.FindAsync(book.AuthorId);
            book.Category = await _appDbContext.Categories.FindAsync(book.CategoryId);
        }
        Console.WriteLine("Total hits: " + response.Total);
        return books;
    }
}
