using System.Security.Cryptography;
using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Core.Reports;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        // GET /api/products
        group.MapGet("/", (ICatalogStore store) =>
            TypedResults.Ok(store.List().Select(ProductDto.From).ToList()));

        // GET /api/products/low-stock?threshold=5  (додатковий маршрут домену "Склад")
        group.MapGet("/low-stock", (int? threshold, ICatalogStore store) =>
            TypedResults.Ok(ReportService.LowStock(store.List(), threshold ?? 5)));

        // GET /api/products/{id}
        group.MapGet("/{id}", Results<Ok<ProductDto>, NotFound> (string id, ICatalogStore store) =>
            store.GetById(id) is { } found
                ? TypedResults.Ok(ProductDto.From(found))
                : TypedResults.NotFound());

        // POST /api/products
        group.MapPost("/", Results<Created<ProductDto>, ValidationProblem>
            (SaveProductRequest request, ICatalogStore store) =>
        {
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var created = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity);
            store.Add(created);

            return TypedResults.Created($"/api/products/{created.Id}", ProductDto.From(created));
        });

        // PUT /api/products/{id}
        group.MapPut("/{id}", Results<Ok<ProductDto>, NotFound, ValidationProblem>
            (string id, SaveProductRequest request, ICatalogStore store) =>
        {
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);
            if (store.GetById(id) is null)
                return TypedResults.NotFound();

            // PUT повністю замінює стан: створюємо новий валідний Product з тим самим id
            var replacement = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity);
            store.Update(replacement);

            return TypedResults.Ok(ProductDto.From(replacement));
        });

        // DELETE /api/products/{id}
        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, ICatalogStore store) =>
            store.Remove(id) ? TypedResults.NoContent() : TypedResults.NotFound());
    }
}
