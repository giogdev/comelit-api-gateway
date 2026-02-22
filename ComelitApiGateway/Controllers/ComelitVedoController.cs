using ComelitApiGateway.Commons.Dtos.Vedo;
using ComelitApiGateway.Commons.Enums.Vedo;
using ComelitApiGateway.Commons.Interfaces;
using ComelitApiGateway.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ComelitApiGateway.Controllers
{
    /// <summary>
    /// Comelit "Vedo" (alarm) integration apis
    /// </summary>
    /// <param name="config"></param>
    /// <param name="vedo"></param>
    [ApiController]
    [Route("vedo")]
    public class ComelitVedoController(IConfiguration config, IComelitVedo vedo) : BaseController(config)
    {
        protected readonly IComelitVedo _vedo = vedo;

        #region GET

        /// <summary>
        /// Get general status of alarm
        /// </summary>
        /// <returns></returns>
        [HttpGet("status")]
        [ProducesResponseType(typeof(VedoStatusModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGeneralStatus()
        {
            try
            {
                var areas = await _vedo.GetAreasStatus();
                if (areas.Any(x => x.Alarm))
                {
                    return Ok(new
                    {
                        Id = AlarmStatusEnum.Alarm,
                        Description = AlarmStatusEnum.Alarm.ToString()
                    });
                }
                else if (areas.All(x => x.Status == AlarmStatusEnum.Active))
                {
                    return Ok(new
                    {
                        Id = AlarmStatusEnum.Active,
                        Description = AlarmStatusEnum.Active.ToString()
                    });
                }
                else if (areas.Any(x => x.Armed))
                {
                    return Ok(new
                    {
                        Id = AlarmStatusEnum.PartialActive,
                        Description = AlarmStatusEnum.PartialActive.ToString()
                    });
                }
                else
                {
                    return Ok(new VedoStatusModel()
                    {
                        Id = AlarmStatusEnum.NotEntered,
                        Description = AlarmStatusEnum.NotEntered.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                ManageException(ex);

                return Ok(new VedoStatusModel
                {
                    Id = AlarmStatusEnum.Unknown,
                    Description = AlarmStatusEnum.Unknown.ToString()
                });
            }
        }

        /// <summary>
        /// Return list of areas with alarm status
        /// </summary>
        /// <returns></returns>
        [HttpGet("areas")]
        [ProducesResponseType(typeof(List<VedoAreaStatusDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatusAsync()
        {
            try
            {
                return Ok(await _vedo.GetAreasStatus());
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Get status of specific area
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        [HttpGet("areas/{areaId}")]
        [ProducesResponseType(typeof(VedoAreaStatusDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetAreaStatus(int areaId)
        {
            try
            {
                return Ok((await _vedo.GetAreasStatus()).FirstOrDefault(x => x.Id == areaId));
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Return true if alarm of area is enabled 
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        [HttpGet("areas/{areaId}/is-active")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IsAlarmAreaActive(int areaId)
        {
            try
            {
                var areaStatus = (await _vedo.GetAreasStatus()).FirstOrDefault(x => x.Id == areaId);
                if (areaStatus == null) return Ok(false);
                return Ok(areaStatus.Status == AlarmStatusEnum.Active || areaStatus.Status == AlarmStatusEnum.Activating);

            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        ///  Return true if alarm of all areas is enabled 
        /// </summary>
        /// <returns></returns>
        [HttpGet("areas/all/is-active")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IsAlarmActive()
        {
            try
            {
                return Ok((await _vedo.GetAreasStatus()).Any(x => x.Status == AlarmStatusEnum.Active || x.Status == AlarmStatusEnum.PartialActive));
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get list of zones of specific area
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        [HttpGet("areas/{areaId}/zones")]
        [ProducesResponseType(typeof(List<VedoZoneDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetZonesStatus(int areaId)
        {
            try
            {
                return Ok(await _vedo.GetZoneList(areaId));
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Get list of all zones (section of area)
        /// </summary>
        /// <returns></returns>
        [HttpGet("areas/all/zones")]
        [ProducesResponseType(typeof(List<VedoZoneDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllZonesStatus()
        {
            try
            {
                return Ok(await _vedo.GetZoneList(-1));
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }

        }

        #endregion

        #region CRUD

        /// <summary>
        /// Enable alarm of all areas
        /// </summary>
        /// <returns></returns>
        [HttpPost("areas/all/arm")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ArmAllAreas()
        {
            try
            {
                return Ok(await _vedo.ArmAlarm());
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }
           
        }

        /// <summary>
        /// Enable alarm of specific area
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        [HttpPost("areas/{areaId}/arm")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ArmArea(int areaId)
        {
            try
            {
                return Ok(await _vedo.ArmAlarm(areaId));
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }
            
        }

        /// <summary>
        /// Disable alarm of all areas
        /// </summary>
        /// <returns></returns>
        [HttpPost("areas/all/disarm")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DisarmAllAreas()
        {
            try
            {
                return Ok(await _vedo.DisarmAlarm());
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }
           
        }

        /// <summary>
        /// Disable alarm of specific area
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        [HttpPost("areas/{areaId}/disarm")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DisarmArea(int areaId)
        {
            try
            {
                return Ok(await _vedo.DisarmAlarm(areaId));
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }
            
        }

        /// <summary>
        /// Disable alarm of all areas
        /// </summary>
        /// <returns>True = alarm inserted, False = alarm disabled</returns>
        [HttpPost("areas/all/arm-disarm")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ArmDisarmAllAreas()
        {
            try
            {
                if ((await _vedo.GetAreasStatus()).Any(x => x.Armed))
                {
                    await _vedo.DisarmAlarm();
                    return Ok(false);
                }
                else
                {
                    await _vedo.ArmAlarm();
                    return Ok(true);
                }
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }
            
        }

        /// <summary>
        /// Disable alarm of specific area
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        [HttpPost("areas/{areaId}/arm-disarm")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ArmDisarmAllAreas(int areaId)
        {
            try
            {
                var area = (await _vedo.GetAreasStatus()).FirstOrDefault(x => x.Id == areaId);
                if (area != null)
                {
                    if (area.Armed)
                    {
                        await _vedo.DisarmAlarm(areaId);
                        return Ok(false);
                    }
                    else
                    {
                        await _vedo.ArmAlarm(areaId);
                        return Ok(true);
                    }
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Exclude zone from area (ex. windows, door, sensor, etc..).
        /// </summary>
        /// <param name="zoneId"></param>
        /// <returns></returns>
        [HttpPost("zones/{zoneId}/exclude")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExcludeZone(int zoneId)
        {
            try
            {
                return Ok(await _vedo.ExcludeZone(zoneId));
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Include zone that has been excluded
        /// </summary>
        /// <param name="zoneId"></param>
        /// <returns></returns>
        [HttpPost("zones/{zoneId}/include")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IncludeZone(int zoneId)
        {
            try
            {
                return Ok(await _vedo.IncludeZone(zoneId));
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Isolate zone
        /// </summary>
        /// <param name="zoneId"></param>
        /// <returns></returns>
        [HttpPost("zones/{zoneId}/isolate")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IsolateZone(int zoneId)
        {
            try
            {
                return Ok(await _vedo.IsolateZone(zoneId));
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Remove zone from list of isolated devices
        /// </summary>
        /// <param name="zoneId"></param>
        /// <returns></returns>
        [HttpPost("zones/{zoneId}/remove-isolate")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UnisolateZone(int zoneId)
        {
            try
            {
                return Ok(await _vedo.UnisolateZone(zoneId));
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return BadRequest(ex.Message);
            }

        }

        #endregion
    }
}
