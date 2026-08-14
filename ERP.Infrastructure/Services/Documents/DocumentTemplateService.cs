using System.Text;
using ERP.Application.Common.Documents;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Services.Documents;

/// <summary>
/// Stub implementation of document template service.
/// In production, this would use a proper template engine like Handlebars or Razor.
/// </summary>
public class DocumentTemplateService : IDocumentTemplateService
{
    private readonly ILogger<DocumentTemplateService> _logger;

    public DocumentTemplateService(ILogger<DocumentTemplateService> logger)
    {
        _logger = logger;
    }

    public Task<byte[]> GeneratePayslipAsync(PayslipTemplateData data, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating payslip for {Employee}", data.EmployeeName);

        var html = GeneratePayslipHtml(data);
        return Task.FromResult(Encoding.UTF8.GetBytes(html));
    }

    public Task<byte[]> GenerateLeaveApprovalLetterAsync(LeaveLetterData data, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating leave letter for {Employee}", data.EmployeeName);

        var html = GenerateLeaveLetterHtml(data);
        return Task.FromResult(Encoding.UTF8.GetBytes(html));
    }

    public Task<byte[]> GenerateEmploymentContractAsync(EmploymentContractData data, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating employment contract for {Employee}", data.EmployeeName);

        var html = GenerateEmploymentContractHtml(data);
        return Task.FromResult(Encoding.UTF8.GetBytes(html));
    }

    public Task<byte[]> GenerateCertificateOfEmploymentAsync(CertificateOfEmploymentData data, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating COE for {Employee}", data.EmployeeName);

        var html = GenerateCertificateHtml(data);
        return Task.FromResult(Encoding.UTF8.GetBytes(html));
    }

    public Task<byte[]> GenerateTaxCertificateAsync(TaxCertificateData data, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating tax certificate for {Employee}", data.EmployeeName);

        var html = GenerateTaxCertificateHtml(data);
        return Task.FromResult(Encoding.UTF8.GetBytes(html));
    }

    public Task<byte[]> GenerateFromTemplateAsync(string templateId, Dictionary<string, object> data, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating document from template {TemplateId}", templateId);

        // Stub - would use template engine
        return Task.FromResult(Encoding.UTF8.GetBytes($"<html><body><h1>Template: {templateId}</h1><pre>{System.Text.Json.JsonSerializer.Serialize(data)}</pre></body></html>"));
    }

    private string GeneratePayslipHtml(PayslipTemplateData d)
    {
        var monthName = new DateTime(d.Year, d.Month, 1).ToString("MMMM yyyy");

        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Slip Gaji - {d.EmployeeName}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; font-size: 14px; }}
        .header {{ text-align: center; margin-bottom: 30px; border-bottom: 2px solid #333; padding-bottom: 20px; }}
        .header h1 {{ margin: 0; color: #333; }}
        .meta {{ color: #666; margin-top: 5px; }}
        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        th, td {{ padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }}
        th {{ background-color: #f5f5f5; }}
        .amount {{ text-align: right; }}
        .total-row {{ font-weight: bold; background-color: #f5f5f5; }}
        .net {{ font-size: 18px; color: #4a90d9; }}
        .footer {{ margin-top: 40px; text-align: center; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>{d.CompanyName}</h1>
        <div class='meta'>{d.CompanyAddress}</div>
        <h2>SLIP GAJI</h2>
        <div class='meta'>Periode: {monthName}</div>
    </div>

    <table>
        <tr><td><strong>NIK:</strong></td><td>{d.EmployeeNik}</td></tr>
        <tr><td><strong>Nama:</strong></td><td>{d.EmployeeName}</td></tr>
        <tr><td><strong>Departemen:</strong></td><td>{d.Department}</td></tr>
        <tr><td><strong>Posisi:</strong></td><td>{d.Position}</td></tr>
        <tr><td><strong>Tanggal Bayar:</strong></td><td>{d.PaymentDate:dd MMMM yyyy}</td></tr>
        <tr><td><strong>Rekening:</strong></td><td>{d.BankAccount}</td></tr>
    </table>

    <h3>PENGHASILAN</h3>
    <table>
        <thead><tr><th>Kode</th><th>Keterangan</th<th class='amount'>Jumlah</th></tr></thead>
        <tbody>
            {string.Join("", d.Earnings.Select(e => $"<tr><td>{e.Code}</td><td>{e.Name}</td><td class='amount'>{e.Amount:N2}</td></tr>"))}
            <tr class='total-row'><td></td><td><strong>Total Penghasilan</strong></td><td class='amount'>{d.TotalEarnings:N2}</td></tr>
        </tbody>
    </table>

    <h3>POTONGAN</h3>
    <table>
        <thead><tr><th>Kode</th><th>Keterangan</th><th class='amount'>Jumlah</th></tr></thead>
        <tbody>
            {string.Join("", d.Deductions.Select(d => $"<tr><td>{d.Code}</td><td>{d.Name}</td><td class='amount'>{d.Amount:N2}</td></tr>"))}
            <tr class='total-row'><td></td><td><strong>Total Potongan</strong></td><td class='amount'>{d.TotalDeductions:N2}</td></tr>
        </tbody>
    </table>

    <table style='margin-top: 30px;'>
        <tr class='total-row net'>
            <td></td><td><strong>GAJI BERSIH</strong></td>
            <td class='amount'><strong>Rp {d.NetSalary:N2}</strong></td>
        </tr>
    </table>

    <div class='footer'>
        <p>Dokumen ini dicetak secara otomatis pada {DateTime.UtcNow:dd MMMM yyyy HH:mm} WIB</p>
    </div>
</body>
</html>";
    }

    private string GenerateLeaveLetterHtml(LeaveLetterData d)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Surat Izin - {d.EmployeeName}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; font-size: 14px; line-height: 1.6; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .content {{ text-align: justify; margin: 20px 0; }}
        .signature {{ margin-top: 60px; text-align: right; }}
    </style>
</head>
<body>
    <div class='header'>
        <h2>{d.CompanyName}</h2>
        <p>{d.CompanyAddress}</p>
        <hr>
        <h3>SURAT IZINCUTI</h3>
    </div>

    <div class='content'>
        <p>Yang bertanda tangan di bawah ini, memberikan izin cuti kepada:</p>
        <table style='margin: 20px 0;'>
            <tr><td style='width: 150px;'>Nama</td><td>: {d.EmployeeName}</td></tr>
            <tr><td>NIK</td><td>: {d.EmployeeNik}</td></tr>
            <tr><td>Departemen</td><td>: {d.Department}</td></tr>
            <tr><td>Jenis Cuti</td><td>: {d.LeaveType}</td></tr>
            <tr><td>Tanggal Mulai</td><td>: {d.StartDate:dd MMMM yyyy}</td></tr>
            <tr><td>Tanggal Selesai</td><td>: {d.EndDate:dd MMMM yyyy}</td></tr>
            <tr><td>Jumlah Hari</td><td>: {d.TotalDays} hari</td></tr>
            <tr><td>Keterangan</td><td>: {d.Reason}</td></tr>
        </table>

        <p>Cuti tersebut disetujui pada tanggal {d.ApprovedDate:dd MMMM yyyy}</p>

        <p>Demikian surat izin cuti ini dibuat untuk dapat digunakan sebagaimana mestinya.</p>
    </div>

    <div class='signature'>
        <p>Jakarta, {DateTime.UtcNow:dd MMMM yyyy}</p>
        <p>Pemberi Izin,</p>
        <br><br><br>
        <p><strong>{d.ApprovedByName}</strong></p>
    </div>
</body>
</html>";
    }

    private string GenerateEmploymentContractHtml(EmploymentContractData d)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Kontrak Kerja - {d.EmployeeName}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; font-size: 14px; line-height: 1.8; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .content {{ text-align: justify; }}
        .signature {{ margin-top: 60px; display: flex; justify-content: space-between; }}
    </style>
</head>
<body>
    <div class='header'>
        <h2>SURAT PERJANJIAN KERJA</h2>
        <p>Nomor: SPK/{DateTime.UtcNow:yyyyMMdd}/001</p>
    </div>

    <div class='content'>
        <p>Pada hari ini, {DateTime.UtcNow:dddd} tanggal {DateTime.UtcNow:dd MMMM yyyy}, kami yang bertanda tangan di bawah ini:</p>

        <h4>PIHAK PERTAMA (PERUSAHAAN)</h4>
        <p>{d.CompanyName}<br>
        Alamat: {d.CompanyAddress}<br>
        NPWP: {d.CompanyNpwp}</p>

        <h4>PIHAK KEDUA (PEKERJA)</h4>
        <p>Nama: {d.EmployeeName}<br>
        NIK: {d.EmployeeNik}<br>
        Alamat: {d.EmployeeAddress}<br>
        NPWP: {d.EmployeeNpwp}</p>

        <p>Kedua belah pihak sepakat untuk membuat Perjanjian Kerja dengan ketentuan sebagai berikut:</p>

        <ol>
            <li><strong>Jabatan:</strong> {d.Position}</li>
            <li><strong>Jenis Pekerjaan:</strong> {d.EmploymentType}</li>
            <li><strong>Tanggal Mulai:</strong> {d.ContractStartDate:dd MMMM yyyy}</li>
            <li><strong>Tanggal Berakhir:</strong> {d.ContractEndDate:dd MMMM yyyy}</li>
            <li><strong>Gaji Pokok:</strong> Rp {d.BasicSalary:N2} per bulan</li>
            <li><strong>Jadwal Kerja:</strong> {d.WorkSchedule}</li>
        </ol>

        <p>Surat Perjanjian Kerja ini dibuat dalam rangkap 2 (dua) dan ditandatangani pada tanggal {d.SignedDate:dd MMMM yyyy}.</p>
    </div>

    <div class='signature'>
        <div>
            <p>PIHAK PERTAMA</p>
            <br><br><br>
            <p>____________________</p>
        </div>
        <div>
            <p>PIHAK KEDUA</p>
            <br><br><br>
            <p>{d.EmployeeName}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateCertificateHtml(CertificateOfEmploymentData d)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Surat Keterangan Kerja - {d.EmployeeName}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; font-size: 14px; line-height: 1.8; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .content {{ text-align: justify; }}
        .signature {{ margin-top: 60px; text-align: center; }}
    </style>
</head>
<body>
    <div class='header'>
        <h2>{d.CompanyName}</h2>
        <p>{d.CompanyAddress}</p>
        <hr>
        <h3>SURAT KETERANGAN KERJA</h3>
        <p>Nomor: SKK/{DateTime.UtcNow:yyyyMMdd}/001</p>
    </div>

    <div class='content'>
        <p>Dengan ini kami menyatakan bahwa:</p>

        <table style='margin: 20px 0;'>
            <tr><td style='width: 150px;'>Nama</td><td>: {d.EmployeeName}</td></tr>
            <tr><td>NIK</td><td>: {d.EmployeeNik}</td></tr>
            <tr><td>Departemen</td><td>: {d.Department}</td></tr>
            <tr><td>Jabatan</td><td>: {d.Position}</td></tr>
            <tr><td>Tanggal Masuk</td><td>: {d.JoinDate:dd MMMM yyyy}</td></tr>
            { (d.EndDate.HasValue ? $"<tr><td>Tanggal Selesai</td><td>: {d.EndDate:dd MMMM yyyy}</td></tr>" : "")}
        </table>

        <p>adalah benar merupakan karyawan {d.CompanyName} dengan status kepegawaian yang masih aktif.</p>

        <p>Surat Keterangan Kerja ini dibuat untuk keperluan {d.EmployeeName} dan diberikan pada tanggal {d.IssueDate:dd MMMM yyyy}.</p>

        <p>Demikian surat keterangan ini dibuat dengan sebenarnya.</p>
    </div>

    <div class='signature'>
        <br><br><br>
        <p><strong>{d.SignatoryName}</strong></p>
        <p>{d.SignatoryPosition}</p>
    </div>
</body>
</html>";
    }

    private string GenerateTaxCertificateHtml(TaxCertificateData d)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Bukti Pemotongan Pajak - {d.EmployeeName}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; font-size: 14px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        th, td {{ padding: 8px; text-align: left; border: 1px solid #ddd; }}
        th {{ background-color: #f5f5f5; }}
        .amount {{ text-align: right; }}
    </style>
</head>
<body>
    <div class='header'>
        <h2>BUKTI PEMOTONGAN PAJAK PENGHASILAN</h2>
        <h3>Pasal 21</h3>
        <p>Tahun Pajak {d.TaxYear}</p>
    </div>

    <table>
        <tr><td><strong>Penghasilan Bruto</strong></td><td class='amount'>Rp {d.GrossIncome:N2}</td></tr>
        <tr><td><strong>Total Pengurangan</strong></td><td class='amount'>Rp {d.TotalDeductions:N2}</td></tr>
        <tr><td><strong>PTKP</strong></td><td class='amount'>Rp {d.Ptkp:N2}</td></tr>
        <tr><td><strong>Penghasilan Kena Pajak</strong></td><td class='amount'>Rp {d.TaxableIncome:N2}</td></tr>
        <tr><td><strong>PPh Pasal 21 Tahunan</strong></td><td class='amount'>Rp {d.Pph21Annual:N2}</td></tr>
        <tr><td><strong>PPh Pasal 21 Bulanan</strong></td><td class='amount'>Rp {d.Pph21Monthly:N2}</td></tr>
    </table>

    <p style='margin-top: 30px;'><strong>Data Pemberi Kerja:</strong></p>
    <p>{d.CompanyName}<br>NPWP: {d.CompanyNpwp}<br>{d.CompanyAddress}</p>

    <p><strong>Data Karyawan:</strong></p>
    <p>{d.EmployeeName}<br>NIK: {d.EmployeeNik}<br>NPWP: {d.EmployeeNpwp}</p>

    <p style='margin-top: 30px;'>Diterbitkan pada {d.IssueDate:dd MMMM yyyy}</p>
</body>
</html>";
    }
}
