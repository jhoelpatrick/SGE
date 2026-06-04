using System;
using System.Threading.Tasks;
using SGE.Models;
using SGE.Repositories;

namespace SGE.Services
{
    public class CompaniaService
    {
        private readonly CompaniaRepository _repository;

        public CompaniaService(CompaniaRepository repository)
        {
            _repository = repository;
        }

        public async Task<ModelCompania> GetCompaniaActivaAsync()
        {
            return await _repository.ObtenerCompaniaActivaAsync();
        }

        public async Task<bool> SaveCompaniaAsync(ModelCompania compañia)
        {
            if (string.IsNullOrWhiteSpace(compañia.razon_social))
                throw new ArgumentException("La Razón Social es requerida.");

            if (string.IsNullOrWhiteSpace(compañia.RUC))
                throw new ArgumentException("El RUC es requerido.");

            if (string.IsNullOrWhiteSpace(compañia.Direc_Fiscal))
                throw new ArgumentException("La Dirección Fiscal es requerida.");

            return await _repository.GuardarCompaniaAsync(compañia);
        }
    }
}