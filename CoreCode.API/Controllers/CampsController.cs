using AutoMapper;
using CoreCode.API.Data.Entities;
using CoreCode.API.Models;
using CoreCodeCamp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCode.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository repository, IMapper mapper,LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }


        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsAsync(includeTalks);

                return _mapper.Map<CampModel[]>(results);

            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failide" + ex);

            }


        }
        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var result = await _repository.GetCampAsync(moniker);
                if (result == null)
                {

                    return NotFound();
                }
                return _mapper.Map<CampModel>(result);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failide" + ex);
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsByEventDate(theDate,includeTalks);
                if (!results.Any()) return NotFound();


                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception ex)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failide" + ex);
            }
        }

        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var existing = await _repository.GetCampAsync(model.Moniker);
                if (existing!=null)
                {
                    return BadRequest("Moniker in use");
                }

                var location = _linkGenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker });
                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("could not use current moniler");
                }
                var campSnapshot = _mapper.Map<Camp>(model);
                _repository.Add(campSnapshot);

                if (await _repository.SaveChangesAsync())
                {
                    return Created($"/api/camps/{campSnapshot.Moniker}", _mapper.Map<CampModel>(campSnapshot));
                }
                
            }
            catch (Exception ex)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failide" + ex);
            }
            return BadRequest();

        }
        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model) 
        {
            try
            {
                var modelSnaphot = await _repository.GetCampAsync(model.Moniker);
                if (modelSnaphot == null) return NotFound($"could not found camp with moniker of {moniker}");
                //taking from and applying to dastination and it apply it to the db
                _mapper.Map(model, modelSnaphot);
                
                if (await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<CampModel>(modelSnaphot);
                }

            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failide" + ex);
                
            }
            return BadRequest();
        }
        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            //IActionresult because it's only gonna return an status code there is not body one we delete an item
            try
            {

                var snapShot = await _repository.GetCampAsync(moniker);
                if (snapShot==null) return NotFound("could not found the moniker");

                _repository.Delete(snapShot);
                if (await _repository.SaveChangesAsync())
                {
                    return Ok();
                }


            }
            catch (Exception ex)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failide" + ex);
            }

            return BadRequest("Faild to delete the camp");
        }

    }
}
