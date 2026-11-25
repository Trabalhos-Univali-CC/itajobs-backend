using System.Net;
using System.Net.Mail;

namespace Itajobs_API.src.Services
{
    /// <summary>Envio de e-mail. Nunca lanca excecao: falha vira log.</summary>
    public interface IEmailService
    {
        Task EnviarAsync(string para, string assunto, string corpoTexto);
    }

    /// <summary>Configuracao do SMTP, lida do itajobs.env (<c>SMTP_*</c>).</summary>
    public record SmtpOptions(string Host, int Porta, bool Ssl, string? Usuario, string? Senha, string Remetente);

    /// <summary>Envia pelo SMTP configurado em <c>SMTP_HOST</c> e afins.</summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(SmtpOptions options, ILogger<SmtpEmailService> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task EnviarAsync(string para, string assunto, string corpoTexto)
        {
            try
            {
                using var cliente = new SmtpClient(_options.Host, _options.Porta) { EnableSsl = _options.Ssl };
                if (!string.IsNullOrEmpty(_options.Usuario))
                {
                    cliente.Credentials = new NetworkCredential(_options.Usuario, _options.Senha);
                }

                using var mensagem = new MailMessage(_options.Remetente, para, assunto, corpoTexto);
                await cliente.SendMailAsync(mensagem);
            }
            catch (Exception e)
            {
                // Sem o endereco no log: e dado pessoal.
                _logger.LogError(e, "Falha ao enviar e-mail \"{Assunto}\".", assunto);
            }
        }
    }

    /// <summary>
    /// Sem SMTP configurado. Em desenvolvimento, escreve o e-mail no log (para testar
    /// a recuperacao de senha sem servidor de e-mail); fora dele, so avisa que nada foi
    /// enviado, sem expor o conteudo (que tem o link de troca de senha).
    /// </summary>
    public class LogEmailService : IEmailService
    {
        private readonly ILogger<LogEmailService> _logger;
        private readonly bool _desenvolvimento;

        public LogEmailService(ILogger<LogEmailService> logger, IHostEnvironment ambiente)
        {
            _logger = logger;
            _desenvolvimento = ambiente.IsDevelopment();
        }

        public Task EnviarAsync(string para, string assunto, string corpoTexto)
        {
            if (_desenvolvimento)
            {
                _logger.LogWarning("SMTP nao configurado. E-mail que seria enviado:\nAssunto: {Assunto}\n{Corpo}",
                    assunto, corpoTexto);
            }
            else
            {
                _logger.LogError("SMTP nao configurado (SMTP_HOST no itajobs.env): e-mail \"{Assunto}\" nao foi enviado.",
                    assunto);
            }

            return Task.CompletedTask;
        }
    }
}
