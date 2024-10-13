using Microsoft.AspNetCore.Mvc;
using StoreApi.Models;
using StoreApi.Services;

namespace StoreApi.Controllers;

[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _database;
    private readonly IWebHostEnvironment _env;

    private readonly List<string> _listCategories = new List<string>()
    {
        "Phones", "Computers", "Accessories", "Printers", "Cameras", "Other"
    };

    public ProductsController(ApplicationDbContext database, IWebHostEnvironment env)
    {
        _database = database;
        _env = env;
    }

    [HttpGet("Categories")]
    public IActionResult GetCategories()
    {
        return Ok(_listCategories);
    }

    [HttpGet("GetProducts")]
    public IActionResult GetProducts(string? search, string? category, int? minPrice, int? maxPrice
    , string? sort,string? order, int? page)
    {
        IQueryable<Product> query = _database.Products;

        if (search != null)
        {
            query = query.Where(x => x.Description.Contains(search) || x.Name.Contains(search) );
        }

        if (category != null)
        {
            query = query.Where(x => x.Category == category);
        }

        if (minPrice != null)
        {
            query = query.Where(x => x.Price >= minPrice);
        }

        if (maxPrice != null)
        {
            query = query.Where(x => x.Price <= maxPrice);
        }
        
        // sort functionality
        if (sort == null) sort = "id";
        if (order == null || order != "asc") order = "desc";
        if (sort.ToLower() == "name")
        {
            if (order == "asc")
            {
                query = query.OrderBy(p => p.Name);
            }
            else
            {
                query = query.OrderByDescending(p => p.Name);
            }
        }
        else if (sort.ToLower() == "brand")
        {
            if (order == "asc")
            {
                query = query.OrderBy(p => p.Brand);
            }
            else
            {
                query = query.OrderByDescending(p => p.Brand);
            }
        }
        
        else if (sort.ToLower() == "category")
        {
            if (order == "asc")
            {
                query = query.OrderBy(p => p.Category);
            }
            else
            {
                query = query.OrderByDescending(p => p.Category);
            }
        }
        
        else if (sort.ToLower() == "price")
        {
            if (order == "asc")
            {
                query = query.OrderBy(p => p.Price);
            }
            else
            {
                query = query.OrderByDescending(p => p.Price);
            }
        }
        
        else if (sort.ToLower() == "date")
        {
            if (order == "asc")
            {
                query = query.OrderBy(p => p.CreatedAt);
            }
            else
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }
        }
        
        else 
        {
            if (order == "asc")
            {
                query = query.OrderBy(p => p.Id);
            }
            else
            {
                query = query.OrderByDescending(p => p.Id);
            }
        }

        if (page == null || page < 1) page = 1;

        int pageSize = 5;
        int totalPages = 0;

        decimal count = query.Count();
        totalPages = (int)Math.Ceiling(count / pageSize);

        query = query.Skip((int)(page - 1) * pageSize).Take(pageSize);
        
        var products = query.ToList();
        var result = new
        {
            products = products,
            totalPages = totalPages,
            pageSize = pageSize,
            page = page
        };
        return Ok(result);
    }

    [HttpGet("{id}")]
    public IActionResult GetProduct(int id)
    {
        var result = _database.Products.Find(id);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost("AddProducts")]
    public IActionResult AddProduct([FromForm] ProductDto productDto)
    {
        if (!_listCategories.Contains(productDto.Category))
        {
            ModelState.AddModelError("Category", "Category is not valid!");
            return BadRequest(ModelState);
        }

        if (productDto.ImageFile == null)
        {
            ModelState.AddModelError("ImageFile", "The image is required!");
            return BadRequest(ModelState);
        }

        // save the image on the server
        var imageFileName = DateTime.Now.ToString("yyyy MMMM dd HH mm ss fff");
        imageFileName += Path.GetExtension(productDto.ImageFile.FileName);
        var imagesFolder = _env.WebRootPath + "/images/products/";

        using (var stream = System.IO.File.Create(imagesFolder + imageFileName))
        {
            productDto.ImageFile.CopyTo(stream);
        }

        // save product in the database 
        var product = new Product()
        {
            Name = productDto.Name,
            Brand = productDto.Brand,
            Category = productDto.Category,
            Description = productDto.Description,
            Price = productDto.Price,
            ImageFileName = imageFileName,
            CreatedAt = DateTime.Now
        };
        _database.Products.Add(product);
        _database.SaveChanges();
        return Ok(product);
    }

    [HttpPut("UpdateProduct")]
    public IActionResult UpdateProduct(int id, [FromForm] ProductDto productDto)
    {
        var product = _database.Products.Find(id);

        if (product.Id == null)
        {
            ModelState.AddModelError("UpdateProduct", "Please enter a valid id!");
        }

        if (!_listCategories.Contains(productDto.Category))
        {
            ModelState.AddModelError("Category", "Please Select a Valid Category!");
        }

        var imageFileName = product.ImageFileName;
        if (productDto.ImageFile != null)
        {
            // save image on the server
            imageFileName = DateTime.Now.ToString("yyyy MMMM dd HH mm ss fff");
            imageFileName += Path.GetExtension(productDto.ImageFile.FileName);
            var imagesFolder = _env.WebRootPath + "/images/products/";
            using (var stream = System.IO.File.Create(imagesFolder + imageFileName))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            System.IO.File.Delete(imagesFolder + product.ImageFileName);
        }

        product.Brand = productDto.Brand;
        product.Category = productDto.Category;
        product.Description = productDto.Description;
        product.Name = productDto.Name;
        product.Price = productDto.Price;
        product.ImageFileName = productDto.ImageFile!.FileName;
        return Ok(product);
    }

    [HttpDelete("{id}")]
    public IActionResult RemoveProduct(int id)
    {
        var product = _database.Products.FirstOrDefault(x => x.Id == id);
        if (product?.Id != id)
        {
            ModelState.AddModelError("RemoveProduct",
                $"You Can not remove this product! Because there is no id with id = {id}");
            return BadRequest(ModelState);
        }

        var imageFolder = _env.WebRootPath + "/images/products/";
        System.IO.File.Delete(imageFolder + product.ImageFileName);

        _database.Products.Remove(product);
        _database.SaveChanges();
        return Ok();
    }
}