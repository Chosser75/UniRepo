﻿namespace UniversalRepositoryTests.Models.Entities;

public class TestUserRole
{
    public Guid UserId { get; set; }

    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;
}