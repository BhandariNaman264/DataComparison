var __extends =
  (this && this.__extends) ||
  (function () {
    var extendStatics =
      Object.setPrototypeOf ||
      ({ __proto__: [] } instanceof Array &&
        function (d, b) {
          d.__proto__ = b;
        }) ||
      function (d, b) {
        for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
      };
    return function (d, b) {
      extendStatics(d, b);
      function __() {
        this.constructor = d;
      }
      d.prototype =
        b === null
          ? Object.create(b)
          : ((__.prototype = b.prototype), new __());
    };
  })();
Object.defineProperty(exports, "__esModule", { value: true });
const re =
  /([A-Z])([a-z])+([A-Za-z])+(, )([A-Z])([a-z])+([A-Za-z])+( <(([A-Za-z])+(.)([A-Za-z])+@ceridian.com>))/g;
const reType = /((([A-Za-z])+(.)([A-Za-z])+@ceridian.com))/g;
var React = require("react");
var ReactMultiEmail = /** @class */ (function (_super) {
  __extends(ReactMultiEmail, _super);
  function ReactMultiEmail(props) {
    var _this = _super.call(this, props) || this;
    _this.state = {
      focused: false,
      emails: [],
      inputValue: "",
      start: false,
    };

    _this.findEmailAddress = function (value) {
      var validEmails = [];
      // format for accepted string will be 'LastName, FirstName <Email>;'
      var addEmails = function (email) {
        var emails = _this.state.emails;
        for (var i = 0, l = emails.length; i < l; i++) {
          if (emails[i] === email) {
            return false;
          }
        }
        validEmails.push(email);
        _this.setState({
          emails: _this.state.emails.concat(validEmails),
          inputValue: "",
        });
        return true;
      };

      if (value !== "" && value.includes(">")) {
        if (re.test(value)) {
          var arr = value.split(";").filter(function (n) {
            return n !== "" && n !== undefined && n !== null;
          });
          if (arr.length === 0) {
            // no semi-colo was selected so can just add the one directly
            addEmails(value);
          } else {
            do {
              addEmails(arr.shift());
            } while (arr.length);
          }
        } else {
          if (re.test(value)) {
            addEmails(value + ">");
          }
        }
      } else if (value !== "") {
        if (reType.test(value)) {
          value = value.replace(",", "");
          addEmails(value);
        }
      }
      if (validEmails.length && _this.props.onChange) {
        _this.props.onChange(_this.state.emails.concat(validEmails));
      }
    };

    _this.removeEmail = function (index) {
      _this.setState(
        function (prevState) {
          return {
            emails: prevState.emails
              .slice(0, index)
              .concat(prevState.emails.slice(index + 1)),
          };
        },
        function () {
          if (_this.props.onChange) {
            _this.props.onChange(_this.state.emails);
          }
        }
      );
    };
    _this.handleOnKeydown = function (e) {
      switch (e.which) {
        case 8:
          if (!e.currentTarget.value) {
            _this.removeEmail(_this.state.emails.length - 1);
          }
          break;
        case 13:
        case 9:
        case 186:
          _this.findEmailAddress(e.currentTarget.value);
          break;
        default:
          break;
      }
    };
    _this.handleKeyPress = function (e) {
      var charCode = e.which || e.keyCode;
      var charStr = String.fromCharCode(charCode);
      if (charStr === ">" || charStr === ",") {
        _this.findEmailAddress(e.currentTarget.value, true);
      }
    };
    _this.handleOnChange = function (e) {
      // console.log('test');
      return _this.setState({
        inputValue: e.target.value,
        focused: true,
        start: true,
      });
    };
    _this.handleOnBlur = function (e) {
      _this.setState({ focused: false });
      _this.findEmailAddress(e.currentTarget.value, true);
    };
    _this.handleOnFocus = function () {
      return _this.setState({
        focused: true,
        start: true,
      });
    };
    _this.emailInputRef = React.createRef();
    return _this;
  }
  ReactMultiEmail.getDerivedStateFromProps = function (nextProps, prevState) {
    if (prevState.propsEmails !== nextProps.emails) {
      return {
        propsEmails: nextProps.emails || [],
        emails: nextProps.emails || [],
        inputValue: "",
        focused: true,
      };
    }
    return null;
  };
  ReactMultiEmail.prototype.render = function () {
    var _this = this;
    var _a = this.state,
      focused = _a.focused,
      emails = _a.emails,
      inputValue = _a.inputValue,
      start = _a.start;
    var _b = this.props,
      style = _b.style,
      getLabel = _b.getLabel,
      _c = _b.className,
      className = _c === void 0 ? "" : _c,
      noClass = _b.noClass,
      placeholder = _b.placeholder;
    // removeEmail
    return React.createElement(
      "div",
      {
        className:
          className +
          " " +
          (noClass ? "" : "react-multi-email") +
          " " +
          (focused && start ? "focused" : "") +
          " " +
          (inputValue === "" && emails.length === 0 ? "empty" : ""),
        style: style,
        onClick: function () {
          if (_this.emailInputRef.current) {
            _this.emailInputRef.current.focus();
          }
        },
      },
      placeholder
        ? React.createElement("span", { "data-placeholder": true }, placeholder)
        : null,
      React.createElement(
        "div",
        {
          className: "react-multi-email-outer",
        },
        emails.map(function (email, index) {
          return getLabel(email, index, _this.removeEmail);
        }),
        React.createElement("input", {
          ref: this.emailInputRef,
          type: "text",
          value: inputValue,
          onFocus: this.handleOnFocus,
          onBlur: this.handleOnBlur,
          onChange: this.handleOnChange,
          onKeyPress: this.handleKeyPress,
          onKeyDown: this.handleOnKeydown,
        })
      )
    );
  };
  return ReactMultiEmail;
})(React.Component);
exports.default = ReactMultiEmail;
