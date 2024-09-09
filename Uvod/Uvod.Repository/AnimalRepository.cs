﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uvod.Model;
using Uvod.Repository.Common;


namespace Uvod.Repository
{
    public class AnimalRepository : IAnimalRepository
    {
        private const string connectionString = "Host=localhost:5432;Username=postgres;Password=postgres;Database=WebDatabase";

        public async Task<bool> CreateAnimalAsync(Animal animal)
        {
            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                string commandText = "INSERT INTO \"Animal\"  VALUES (@id, @name, @specise, @age, @dateOfBirth, @ownerId); ";
                var command = new NpgsqlCommand(commandText, connection);

                command.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Uuid, Guid.NewGuid());
                command.Parameters.AddWithValue("@name", animal.Name);
                command.Parameters.AddWithValue("@specise", animal.Specise);
                command.Parameters.AddWithValue("@age", animal.Age);
                command.Parameters.AddWithValue("@dateOfBirth", animal.DateOfBirth);
                command.Parameters.AddWithValue("@ownerId", NpgsqlTypes.NpgsqlDbType.Uuid, animal.OwnerId);

                connection.Open();
                var numberOfCommits = await command.ExecuteNonQueryAsync();
                connection.Close();

                if (numberOfCommits == 0)
                {
                    return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAnimalAsync(Guid id)
        {
            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                string commandText = "DELETE FROM \"Animal\" WHERE \"Id\" = @id;";
                var command = new NpgsqlCommand(commandText, connection);

                command.Parameters.AddWithValue("@id", id);
                connection.Open();

                var numberOfCommits = await command.ExecuteNonQueryAsync();

                if (numberOfCommits == 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<Animal> GetAnimalByIdAsync(Guid id) 
        {
            try
            {
                Animal animal = new Animal();
                Owner owner = new Owner();
                using var connection = new NpgsqlConnection(connectionString);
                string commandText = "SELECT a.\"Id\", a.\"Name\",a.\"Specise\", a.\"Age\",a.\"DateOfBirth\",a.\"OwnerId\",o.\"Id\", o.\"FirstName\", o.\"LastName\" FROM \"Animal\" a LEFT JOIN \"Owner\" o ON a.\"OwnerId\" = o.\"Id\" WHERE a.\"Id\" = @id;";


                var command = new NpgsqlCommand(commandText, connection);

                command.Parameters.AddWithValue("@id", id);
                connection.Open();

                using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    reader.ReadAsync();
                    animal.Id = Guid.Parse(reader["Id"].ToString());
                    animal.Name = reader["Name"].ToString();
                    animal.Specise = reader["Specise"].ToString();
                    animal.Age = Int32.Parse(reader["Age"].ToString());
                    animal.DateOfBirth = DateTime.TryParse(reader["DateOfBirth"].ToString(), out DateTime result) ? result : null;
                    animal.OwnerId = Guid.Parse(reader["OwnerId"].ToString());
                    owner.Id = animal.OwnerId;
                    owner.FirstName = reader["FirstName"].ToString();
                    owner.LastName = reader["LastName"].ToString();
                    animal.Owner = owner;

                }
                else
                {
                    return null;
                }
                return animal;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<Animal>> GetAnimalsAsync()
        {
            try
            {
                List<Animal> animals = new List<Animal>();
                using var connection = new NpgsqlConnection(connectionString);
                string commandText = "SELECT a.\"Id\", a.\"Name\",a.\"Specise\", a.\"Age\",a.\"DateOfBirth\",a.\"OwnerId\",o.\"Id\", o.\"FirstName\", o.\"LastName\" FROM \"Animal\" a LEFT JOIN \"Owner\" o ON a.\"OwnerId\" = o.\"Id\";";

                var command = new NpgsqlCommand(commandText, connection);
                connection.Open();
                using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        Animal animal = new Animal();
                        Owner owner = new Owner();
                        animal.Id = Guid.Parse(reader["Id"].ToString());
                        animal.Name = reader["Name"].ToString();
                        animal.Specise = reader["Specise"].ToString();
                        animal.Age = Int32.Parse(reader["Age"].ToString());
                        animal.DateOfBirth = DateTime.TryParse(reader["DateOfBirth"].ToString(), out DateTime result) ? result : null;
                        animal.OwnerId = Guid.Parse(reader["OwnerId"].ToString());
                        owner.Id = animal.OwnerId;
                        owner.FirstName = reader["FirstName"].ToString();
                        owner.LastName = reader["LastName"].ToString();
                        animal.Owner = owner;
                        animals.Add(animal);
                    }
                }
                else
                {
                    return null;
                }
                return animals;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> UpdateAnimalAsync(Guid id, AnimalUpdate animal)
        {
            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                //string commandText = "UPDATE \"Animal\" SET \"Age\" = @age WHERE \"Id\" = @id;";
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("UPDATE \"Animal\" SET");
                //var command = new NpgsqlCommand(commandText, connection);
                var command = new NpgsqlCommand(); 
                command.Connection = connection;

                if (animal.Name != null)
                {
                    stringBuilder.Append("\"Name\" = @name, ");
                    command.Parameters.AddWithValue("@name", animal.Name);
                }
                if (animal.Specise != null)
                {
                    stringBuilder.Append("\"Specise\" = @specise, ");
                    command.Parameters.AddWithValue("@specise", animal.Specise);
                }
                if (animal.Age != null)
                {
                    stringBuilder.Append("\"Age\" = @age, ");
                    command.Parameters.AddWithValue("@age", animal.Age);
                }

                stringBuilder.Length -= 2;
                stringBuilder.Append(" WHERE \"Id\" = @id;");
                command.CommandText = stringBuilder.ToString();
                command.Parameters.AddWithValue("@id", id);
                
                connection.Open();

                var numberOfCommits = await command.ExecuteNonQueryAsync();

                if (numberOfCommits == 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
