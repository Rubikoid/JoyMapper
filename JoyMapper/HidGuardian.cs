using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyMapper {
    class HidGuardian {
        private static string root_path = @"SYSTEM\CurrentControlSet\Services\HidGuardian\Parameters";
        private static string process_path = @"SYSTEM\CurrentControlSet\Services\HidGuardian\Parameters\Whitelist";
        private static string storage_value = @"AffectedDevices";

        public void AddDevice(string vendor_id, string product_id) {

        }

        public void RemoveDevice(string vendor_id, string product_id) {

        }

        public static string GenerateDeivceString(string vendor_id, string product_id) {
            // return $""
            return "";
        }
    }
}
