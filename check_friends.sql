-- Check test users
SELECT 'Users:' as Info;
SELECT UserId, Username, DisplayName, Status FROM yourchatapp.Users;

-- Check friendships
SELECT '' as '';
SELECT 'Friendships:' as Info;
SELECT FriendshipId, UserId, FriendUserId, Status FROM yourchatapp.Friendships;

-- Check pending requests (Status = 0 = pending)
SELECT '' as '';
SELECT 'Pending Friend Requests:' as Info;
SELECT f.FriendshipId, u1.Username as Requester, u2.Username as Target, f.Status
FROM yourchatapp.Friendships f
JOIN yourchatapp.Users u1 ON f.UserId = u1.UserId
JOIN yourchatapp.Users u2 ON f.FriendUserId = u2.UserId
WHERE f.Status = 0;
