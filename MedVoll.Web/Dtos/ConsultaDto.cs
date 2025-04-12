using MedVoll.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace MedVoll.Web.Dtos
{
    public class ConsultaDto
    {
        public ConsultaDto()
        {
        }

        public ConsultaDto(
            long Id,
            long IdMedico,
            string Paciente,
            
            DateTime Data,
            Especialidade Especialidade
        )
        {
            this.Id = Id;
            this.IdMedico = IdMedico;
            this.Paciente = Paciente;
            this.Data = Data;
            this.Especialidade = Especialidade;
        }

        public ConsultaDto(Consulta consulta)
        {
            Id = consulta.Id;
            IdMedico = consulta.MedicoId;
            MedicoNome = consulta.Medico.Nome;
            Paciente = consulta.Paciente;
            Data = consulta.Data;
            Especialidade = consulta.Medico.Especialidade;
        }

        public long Id { get; set; }
        public string _method { get; set; }

        [Required(ErrorMessage = "Campo obrigatório")] // Vídeo 4.1 - Validando dados
        public long IdMedico { get; set; }
        [ValidateNever] // Vídeo 4.1 - Validando dados
        public string MedicoNome { get; set; }
        [Required(ErrorMessage = "Campo obrigatório"), StringLength(11, MinimumLength = 11, ErrorMessage = "CPF deve ter 11 digitos")] // Vídeo 4.1 - Validando dados
        public string Paciente { get; set; }
        [Required(ErrorMessage = "Campo obrigatório"), DataType(DataType.DateTime)] // Vídeo 4.1 - Validando dados
        public DateTime Data { get; set; }
        [Required(ErrorMessage = "Campo obrigatório")] // Vídeo 4.1 - Validando dados
        public Especialidade Especialidade { get; set; }
    }
}


