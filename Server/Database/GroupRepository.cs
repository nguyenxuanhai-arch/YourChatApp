using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using YourChatApp.Shared.Models;

namespace YourChatApp.Server.Database
{
    public class GroupRepository
    {
        private readonly DbConnection _dbConnection;

        public GroupRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public int CreateGroup(Group group)
        {
            using (var connection = _dbConnection.OpenConnection())
            {
                // Use explicit INSERT and then SELECT LAST_INSERT_ID() to avoid multi-statement issues
                string insertQuery = @"INSERT INTO `Groups` (`GroupName`, `Description`, `CreatedBy`, `CreatedAt`) 
                                VALUES (@GroupName, @Description, @CreatedBy, @CreatedAt)";

                using (var cmd = new MySqlCommand(insertQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@GroupName", group.GroupName);
                    cmd.Parameters.AddWithValue("@Description", group.Description ?? "");
                    cmd.Parameters.AddWithValue("@CreatedBy", group.CreatedBy);
                    cmd.Parameters.AddWithValue("@CreatedAt", group.CreatedAt);

                    cmd.ExecuteNonQuery();
                }

                // Retrieve last insert id
                int groupId = 0;
                using (var cmd2 = new MySqlCommand("SELECT LAST_INSERT_ID();", connection))
                {
                    var res = cmd2.ExecuteScalar();
                    groupId = Convert.ToInt32(res);
                }

                // Add creator as first member
                AddGroupMember(groupId, group.CreatedBy);

                return groupId;
            }
        }

        public List<Group> GetUserGroups(int userId)
        {
            var groups = new List<Group>();
            using (var connection = _dbConnection.OpenConnection())
            {
                string query = @"SELECT DISTINCT g.* FROM `Groups` AS g
                                INNER JOIN `GroupMembers` AS gm ON g.GroupId = gm.GroupId
                                WHERE gm.UserId = @UserId
                                ORDER BY g.CreatedAt DESC";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            groups.Add(new Group
                            {
                                GroupId = reader.GetInt32("GroupId"),
                                GroupName = reader.GetString("GroupName"),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description"),
                                CreatedBy = reader.GetInt32("CreatedBy"),
                                CreatedAt = reader.GetDateTime("CreatedAt")
                            });
                        }
                    }
                }
            }
            return groups;
        }

        public Group GetGroupById(int groupId)
        {
            using (var connection = _dbConnection.OpenConnection())
            {
                string query = "SELECT * FROM `Groups` WHERE GroupId = @GroupId";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@GroupId", groupId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Group
                            {
                                GroupId = reader.GetInt32("GroupId"),
                                GroupName = reader.GetString("GroupName"),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description"),
                                CreatedBy = reader.GetInt32("CreatedBy"),
                                CreatedAt = reader.GetDateTime("CreatedAt")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool AddGroupMember(int groupId, int userId)
        {
            using (var connection = _dbConnection.OpenConnection())
            {
                string query = @"INSERT INTO `GroupMembers` (`GroupId`, `UserId`, `JoinedAt`) 
                                VALUES (@GroupId, @UserId, @JoinedAt)
                                ON DUPLICATE KEY UPDATE UserId=UserId";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@GroupId", groupId);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@JoinedAt", DateTime.Now);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool RemoveGroupMember(int groupId, int userId)
        {
            using (var connection = _dbConnection.OpenConnection())
            {
                string query = "DELETE FROM `GroupMembers` WHERE GroupId = @GroupId AND UserId = @UserId";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@GroupId", groupId);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<User> GetGroupMembers(int groupId)
        {
            var members = new List<User>();
            using (var connection = _dbConnection.OpenConnection())
            {
                string query = @"SELECT u.* FROM `Users` u
                                INNER JOIN `GroupMembers` gm ON u.UserId = gm.UserId
                                WHERE gm.GroupId = @GroupId
                                ORDER BY gm.JoinedAt";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@GroupId", groupId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            members.Add(new User
                            {
                                UserId = reader.GetInt32("UserId"),
                                Username = reader.GetString("Username"),
                                Email = reader.GetString("Email"),
                                DisplayName = reader.GetString("DisplayName"),
                                CreatedAt = reader.GetDateTime("CreatedAt")
                            });
                        }
                    }
                }
            }
            return members;
        }

        public bool IsGroupMember(int groupId, int userId)
        {
            using (var connection = _dbConnection.OpenConnection())
            {
                string query = "SELECT COUNT(*) FROM `GroupMembers` WHERE GroupId = @GroupId AND UserId = @UserId";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@GroupId", groupId);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public bool DeleteGroup(int groupId)
        {
            using (var connection = _dbConnection.OpenConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Delete group members
                        string deleteMembersQuery = "DELETE FROM `GroupMembers` WHERE GroupId = @GroupId";
                        using (var cmd = new MySqlCommand(deleteMembersQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@GroupId", groupId);
                            cmd.ExecuteNonQuery();
                        }

                        // Delete group messages
                        string deleteMessagesQuery = "DELETE FROM `Messages` WHERE GroupId = @GroupId";
                        using (var cmd = new MySqlCommand(deleteMessagesQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@GroupId", groupId);
                            cmd.ExecuteNonQuery();
                        }

                        // Delete group
                        string deleteGroupQuery = "DELETE FROM `Groups` WHERE GroupId = @GroupId";
                        using (var cmd = new MySqlCommand(deleteGroupQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@GroupId", groupId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
    }
}
