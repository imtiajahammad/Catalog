using System;
using System.Collections.Generic;
using System.Linq;
using Catalog.Dtos;
using Catalog.Entities;
using Catalog.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Controllers{

    // GET/items
    [ApiController]
    [Route("[controller]")]
    public class ItemsController:ControllerBase{

        private readonly IItemsRepository _repository;
        public ItemsController(IItemsRepository repository){
            _repository=repository;
        }


        // GET /items
        [HttpGet]
        public IEnumerable<ItemDto> GetItems()
        {
            var items=_repository.GetItems().Select(item=>item.AsDto());
            return items;
        }

        // GET/ items/{id}
        [HttpGet("{id}")]
        public ActionResult<ItemDto> GetItem(Guid id){
            var item=_repository.GetItem(id);
            if(item==null){
                return NotFound();
            }
            return item.AsDto();
        }
        

    }
}