using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using BCrypt.Net;


[ApiController]
[Route("api/[controller]")]
public class UsersControllers : ControllerBase
{
    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<List<Users>> GetAll()
    {
        try
        {
            List<Users> users = DBServices.GetUsers();
            if (users == null || users.Count == 0)
            {
                return NoContent();  // Return NoContent when there are no users.
            }
            return Ok(users);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{id:int:min(0)}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Users))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetUserById(int id)
    {
        try
        {
            Users user = DBServices.GetUserById(id);
            if (user == null)
            {
                return NotFound(new { message = $"User with id = {id} was not found." });
            }
            return Ok(user);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status400BadRequest, e.Message);
        }
    }

    [HttpGet("email/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Users))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetUserByEmail(string email)
    {
        try
        {
            Users user = DBServices.GetUserByEmail(email);
            if (user == null)
            {
                return NotFound(new { message = $"User with Email = {email} was not found." });
            }
            return Ok(user);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status400BadRequest, e.Message);
        }
    }

    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] Users newUser)
    {
        string result = await DBServices.CreateUsers(newUser);

        if (result == "User created successfully.")
        {
            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
        }
        else
        {
            return BadRequest(new { message = result });
        }
    }



    [HttpPut("{id:int:min(0)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateUser(int id, [FromBody] Users updatedUser)
    {
        var user = DBServices.GetUserById(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        bool result = DBServices.UpdateUser(id, updatedUser);

        if (result)
        {
            return Ok(new { message = "User updated successfully." });
        }
        else
        {
            return BadRequest(new { message = "Failed to update user." });
        }
    }

    [HttpPut("password/{id:int:min(0)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateUserPasswordById(int id, [FromBody] PasswordUpdateModel passwordUpdate)
    {
        try
        {
            // Vérification si l'utilisateur existe
            var user = DBServices.GetUserById(id);
            if (user == null)
            {
                return NotFound(new { message = $"User with id = {id} was not found." });
            }
            // Mise à jour du mot de passe
            bool result = DBServices.UpdateUserPasswordById(id, passwordUpdate.NewPassword);
            if (result)
            {
                return NoContent(); // Code 204 si succès
            }
            else
            {
                return BadRequest(new { message = "Failed to update password." });
            }
        }
        catch (Exception e)
        {
            return BadRequest(new { message = e.Message });
        }
    }

    [HttpDelete("{id:int:min(0)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteUser(int id)
    {
        bool result = DBServices.DeleteUserById(id);
        if (result)
        {
            return Ok(new { message = "User deleted successfully." });
        }
        else
        {
            return NotFound(new { message = "User not found." });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Users))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Login([FromBody] LoginData ld)
    {
        try
        {
            if (ld == null || string.IsNullOrEmpty(ld.UserName) || string.IsNullOrEmpty(ld.Password))
            {
                return BadRequest("Invalid login request.");
            }

            // Call the Login method to verify user credentials
            Users usr = DBServices.Login(ld.UserName, ld.Password);
            if (usr != null)
            {
                return Ok(usr);
            }
            else
            {
                return Unauthorized("Invalid username or password.");
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

}
public class PasswordUpdateModel
{
    public string NewPassword { get; set; }
}

