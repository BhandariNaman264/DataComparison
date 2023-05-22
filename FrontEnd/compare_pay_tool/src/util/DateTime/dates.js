export const dates = {
  convert: function (d) {
    // Converts the date in d to a date-object. The input can be:
    //   a date object: returned without modification
    //  an array      : Interpreted as [year,month,day]. NOTE: month is 0-11.
    //   a number     : Interpreted as number of milliseconds
    //                  since 1 Jan 1970 (a timestamp)
    //   a string     : Any format supported by the javascript engine, like
    //                  'YYYY/MM/DD', 'MM/DD/YYYY', 'Jan 31 2009' etc.
    //  an object     : Interpreted as an object with year, month and date
    //                  attributes.  **NOTE** month is 0-11.
    return d.constructor === Date
      ? d
      : d.constructor === Array
      ? new Date(d[0], d[1], d[2])
      : d.constructor === Number
      ? new Date(d)
      : d.constructor === String
      ? new Date(d)
      : typeof d === "object"
      ? new Date(d.year, d.month, d.date)
      : NaN;
  },
  convertToUTCDate: function (d) {
    // the inputed date will follow a string from the lastRestore date API call as month/day/year
    if (d.constructor === String) {
      let tmp = d.split("/");
      // month will be index 0, day, index 1, year index 2
      return `${tmp[2]}-${tmp[0]}-${tmp[1]}`;
    } else {
      return "NA";
    }
  },
  convertToDateString: function (d) {
    if (d.constructor === String) {
      return this.convert(d).toDateString();
    } else {
      return "NA";
    }
  },
  format: function (dateValue) {
    var date = new Date(dateValue);
    var hours = date.getHours();
    var minutes = date.getMinutes();
    var ampm = hours >= 12 ? "PM" : "AM";
    hours = hours % 12;
    hours = hours ? hours : 12; // the hour '0' should be '12'
    minutes = minutes < 10 ? "0" + minutes : minutes;
    var strTime =
      date.toDateString() + " " + hours + ":" + minutes + " " + ampm;
    return strTime;
  },
  compare: function (a, b) {
    // Compare two dates (could be of any type supported by the convert
    // function above) and returns:
    //  -1 : if a < b
    //   0 : if a = b
    //   1 : if a > b
    // NaN : if a or b is an illegal date
    // NOTE: The code inside isFinite does an assignment (=).
    return isFinite((a = this.convert(a).valueOf())) &&
      isFinite((b = this.convert(b).valueOf()))
      ? (a > b) - (a < b)
      : NaN;
  },
  pad: function (num, size) {
    var s = num + "";
    while (s.length < size) s = "0" + s;
    return s;
  },
};
