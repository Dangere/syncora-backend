using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SyncoraBackend.Data;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.DTOs.Report;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class ReportServices(IMapper mapper, SyncoraDbContext dbContext)
{
    private readonly IMapper _mapper = mapper;
    private readonly SyncoraDbContext _dbContext = dbContext;



    public async Task<Result<string>> StoreErrorReport(SubmitReportDTO reportDTO, int? userId)
    {
        ReportEntity submittedReport = _mapper.Map<SubmitReportDTO, ReportEntity>(reportDTO, opt =>
        {
            opt.Items["UserId"] = userId;
            opt.Items["Type"] = ReportType.Error;
        });

        await _dbContext.Reports.AddAsync(submittedReport);
        await _dbContext.SaveChangesAsync();

        return Result<string>.Success("Report stored.");

    }

    public async Task<Result<string>> StoreBugReport(SubmitReportDTO reportDTO, int? userId)
    {

        ReportEntity submittedReport = _mapper.Map<SubmitReportDTO, ReportEntity>(reportDTO, opt =>
        {
            opt.Items["UserId"] = userId;
            opt.Items["Type"] = ReportType.Bug;
        });

        await _dbContext.Reports.AddAsync(submittedReport);
        await _dbContext.SaveChangesAsync();

        return Result<string>.Success("Report stored.");
    }


}