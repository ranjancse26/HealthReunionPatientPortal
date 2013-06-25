using PatientPortal.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace HealthReunionDataAccess
{
    public enum AppointmentTimeEnum
    {
        [StringValue("8AM-9AM")]
        EightToNineAM =  1,
        [StringValue("9AM-10AM")]
        NineToTenAM = 2,
        [StringValue("10AM-11AM")]
        TenToElevenAM = 3,
        [StringValue("11AM-12PM")]
        ElevenToTwelvePM = 4,
        [StringValue("1PM-2PM")]
        OneToTwoPM = 5,
        [StringValue("2PM-3PM")]
        TwoToThreePM = 6,
        [StringValue("3PM-4PM")]
        ThreeToFourPM = 7,
        [StringValue("4PM-5PM")]
        FourToFivePM = 8,
        [StringValue("5PM-6PM")]
        FiveToSixPM = 9,
        [StringValue("6PM-7PM")]
        SixToSevenPM = 10,
        [StringValue("7PM-8PM")]
        SevenToEightPM = 11
    }

    public class AppointementRepository
    {
        /// <summary>
        /// Add Appointments
        /// </summary>
        /// <param name="appointment"></param>
        public void AddAppointments(List<Appointment> appointments)
        {
            using (var tran = new TransactionScope())
            {
                foreach (var appointment in appointments)
                {
                    using (var dataContext = new HealthReunionEntities())
                    {
                        var dbAppointment = (from app in dataContext.Appointments
                                           where app.AppointmentDate == appointment.AppointmentDate && app.PatientId == appointment.PatientId
                                           && app.ProviderId == appointment.ProviderId && app.Time == appointment.Time
                                           select app).FirstOrDefault();
                        if (dbAppointment == null)
                        {
                            dataContext.Appointments.Add(appointment);
                            dataContext.SaveChanges();
                        }
                    }
                }
                tran.Complete();
            }
        }

        /// <summary>
        /// Fetch all available appointment booking for a Provider based on the given date.
        /// </summary>
        /// <param name="providerId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public List<String> GetAvailableAppointmentBookings(int providerId, string date)
        {
            using (var dataContext = new HealthReunionEntities())
            {
                var parsedDate = DateTime.Parse(date);
                List<String> availableSlots = new List<String>();
                var appointmentEnums = StringValueAttribute.GetStringValues(typeof(AppointmentTimeEnum));
                var matchedAppointmentBookingTime = (from appointment in dataContext.Appointments
                                                     where appointment.AppointmentDate == parsedDate
                                                     && appointment.ProviderId == providerId
                                                     select appointment.Time).ToList();

                foreach (var time in appointmentEnums)
                {
                    if (!matchedAppointmentBookingTime.Contains(time))
                        availableSlots.Add(time);
                }

                return availableSlots;
            }
        }

        /// <summary>
        /// Get all appointments for by Provider Id and Date
        /// </summary>
        /// <param name="patientId"></param>
        /// <param name="appointmentDate"></param>
        /// <returns></returns>
        public List<Appointment> GetAllAppointments(int providerId, string date)
        {
            using (var dataContext = new HealthReunionEntities())
            {
                var parsedDate = DateTime.Parse(date);

                return (from appointment in dataContext.Appointments
                        where appointment.AppointmentDate == parsedDate
                                   && appointment.ProviderId == providerId
                        select appointment).ToList();
            }
        }

        /// <summary>
        /// Get all appointments for by PatientId and Date
        /// </summary>
        /// <param name="patientId"></param>
        /// <param name="appointmentDate"></param>
        /// <returns></returns>
        public List<Appointment> GetAppointmentsByPatientID(int patientId, string date)
        {
            using (var dataContext = new HealthReunionEntities())
            {
                var parsedDate = DateTime.Parse(date);
                int providerId =  GetProviderIdForPatient(patientId);

                return (from appointment in dataContext.Appointments
                        where appointment.AppointmentDate == parsedDate
                                   && appointment.PatientId == patientId
                                   && appointment.ProviderId == providerId
                                   select appointment).ToList();
            }
        }

        /// <summary>
        /// Get provider Id by patient Id
        /// </summary>
        /// <param name="patientId"></param>
        /// <returns></returns>
        private int GetProviderIdForPatient(int patientId)
        {
            return new PatientRepository().GetProviderIdForPatient(patientId);
        }
    }
}
