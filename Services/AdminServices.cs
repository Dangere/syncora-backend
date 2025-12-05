using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SyncoraBackend.Data;

namespace SyncoraBackend.Services;

public class AdminServices(IMapper mapper, SyncoraDbContext dbContext, TokenService tokenService, EmailService emailService, IConfiguration configuration)
{
    private readonly IMapper _mapper = mapper;
    private readonly SyncoraDbContext _dbContext = dbContext;
    private readonly TokenService _tokenService = tokenService;
    private readonly EmailService _emailService = emailService;
    private readonly IConfiguration _config = configuration;

}
