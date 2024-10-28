using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;



[ApiController]
[Route("api/[controller]")]
public class SongsController : ControllerBase
{
    // Get all songs
    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Songs))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetAllSongs()
    {
        try
        {
            List<Songs> songs = DBServices_Song.GetAllSongs();
            return Ok(songs);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    // Get songs by genre (SongTypeID)
    [HttpGet("genre/{genreId}")]
    public IActionResult GetSongsByGenre(string genreId)
    {
        try
        {
            List<Songs> songs = DBServices_Song.GetSongsByGenre(genreId);
            return Ok(songs);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    // Create a new song
    [HttpPost("create")]
    public IActionResult CreateSong([FromBody] Songs newSong)
    {
        bool result = DBServices_Song.CreateSong(newSong);
        if (result)
        {
            return Ok(new { message = "Song created successfully." });
        }
        else
        {
            return BadRequest(new { message = "Failed to create song." });
        }
    }

    // Delete a song by ID
    [HttpDelete("{songId}")]
    public IActionResult DeleteSong(string songId)
    {
        bool result = DBServices_Song.DeleteSong(songId);
        if (result)
        {
            return Ok(new { message = "Song deleted successfully." });
        }
        else
        {
            return NotFound(new { message = "Song not found." });
        }
    }

    // Find a song by name
    [HttpGet("search")]
    public IActionResult FindSongByName([FromQuery] string songName)
    {
        try
        {
            Songs song = DBServices_Song.GetSongByName(songName);
            if (song != null)
            {
                return Ok(song);
            }
            else
            {
                return NotFound(new { message = "Song not found." });
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
