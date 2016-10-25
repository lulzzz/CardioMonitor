using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardioMonitor.Models.Treatment;

namespace CardioMonitor.Repositories.Abstract
{
    public interface ITreatmentsRepository
    {
        List<Treatment> GetTreatments(int patientId);

        void AddTreatment(Treatment treatment);

        void DeleteTreatment(int treatmentId);
    }
}
