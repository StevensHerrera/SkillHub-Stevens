using SkillHub.Application.Common.Models;

namespace SkillHub.Application.Common.Interfaces;

public interface ICertificatePdfGenerator
{
    byte[] Generate(CertificateData data);
}
