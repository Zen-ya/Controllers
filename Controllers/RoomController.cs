using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;




[ApiController]
[Route("api/[controller]")]
public class KaraokeRoomsController : ControllerBase
{
    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<List<KaraokeRooms>> GetAllRooms()
    {
        try
        {
            List<KaraokeRooms> rooms = DBServices_Room.GetAllRooms();
            if (rooms == null || rooms.Count == 0)
            {
                return NoContent();
            }
            return Ok(rooms);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{id:int:min(0)}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(KaraokeRooms))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetRoomById(int id)
    {
        try
        {
            KaraokeRooms room = DBServices_Room.GetRoomById(id);
            if (room == null)
            {
                return NotFound(new { message = $"Room with ID = {id} not found." });
            }
            return Ok(room);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateRoom([FromBody] KaraokeRooms newRoom)
    {
        bool result = DBServices_Room.CreateRoom(newRoom);
        if (result)
        {
            return CreatedAtAction(nameof(GetRoomById), new { id = newRoom.RoomID }, newRoom);
        }
        else
        {
            return BadRequest(new { message = "Failed to create room." });
        }
    }

    [HttpPut("{id:int:min(0)}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpdateRoom(int id, [FromBody] KaraokeRooms updatedRoom)
    {
        var room = DBServices_Room.GetRoomById(id);
        if (room == null)
        {
            return NotFound(new { message = "Room not found." });
        }

        bool result = DBServices_Room.UpdateRoom(id, updatedRoom);

        if (result)
        {
            return Ok(new { message = "Room updated successfully." });
        }
        else
        {
            return BadRequest(new { message = "Failed to update room." });
        }
    }

    [HttpDelete("{id:int:min(0)}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult DeleteRoom(int id)
    {
        bool result = DBServices_Room.DeleteRoomById(id);
        if (result)
        {
            return Ok(new { message = "Room deleted successfully." });
        }
        else
        {
            return NotFound(new { message = "Room not found." });
        }
    }
}
