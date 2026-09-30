using Contracts;
using Contracts.IRepository;
using Contracts.IServices;
using Dto;
using System;
using System.Collections.Generic;
using ExceptionHandler;

namespace Services
{
    public class FinalPurchaseOrderService : IFinalPurchaseOrderService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerManager _logger;

        public FinalPurchaseOrderService(IRepositoryWrapper repositoryWrapper, ILoggerManager logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public List<string> GetPoNumbersByFilters(Guid domainId, PurchaseOrderFilterDto filter, int index = 0, int limit = 10)
        {
            _logger.LogInfo($"[START] Fetching distinct PO numbers by filters for Domain: {domainId} with index: {index}, limit: {limit}");

            List<string> data = _repositoryWrapper.FinalPurchaseOrder.GetDistinctPoNumbersByFilters(domainId, filter, index, limit);

                if (data == null || data.Count < 1)
                {
                    _logger.LogError("No PO numbers found for provided filters.");
                    throw new NotFoundCustomException("PO Numbers Not Found", "No PO numbers found for the provided filters.");
                }

            _logger.LogInfo($"[END] Returned {data.Count} PO numbers");
            return data;
        }

        public List<FinalPurchaseOrderDto> GetFinalPurchaseOrdersByPoNumbers(Guid domainId, List<string> poNumbers)
        {
            _logger.LogInfo($"[START] Fetching FinalPurchaseOrder details for Domain: {domainId} and PO Numbers: {string.Join(",", poNumbers)}");

            if (poNumbers == null || poNumbers.Count < 1)
            {
                _logger.LogError("No PO numbers provided.");
                throw new NotFoundCustomException("Invalid Request", "PO numbers list cannot be empty.");
            }

            List<FinalPurchaseOrderDto> results = _repositoryWrapper.FinalPurchaseOrder.GetFinalPurchaseOrdersByPoNumbers(domainId, poNumbers);

            if (results == null || results.Count < 1)
            {
                _logger.LogError("No FinalPurchaseOrder records found for provided PO numbers.");
                throw new NotFoundCustomException("PO Details Not Found", "No purchase order details found for the provided PO numbers.");
            }

            _logger.LogInfo($"[END] Returned {results.Count} FinalPurchaseOrder records");
            return results;
        }
    }
}