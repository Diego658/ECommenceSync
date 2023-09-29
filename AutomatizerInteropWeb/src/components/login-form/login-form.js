import React from 'react';
import TextBox from 'devextreme-react/text-box';
import ValidationGroup from 'devextreme-react/validation-group';
import Validator, { RequiredRule } from 'devextreme-react/validator';
import Button from 'devextreme-react/button';
import CheckBox from 'devextreme-react/check-box';
import './login-form.scss';
import appInfo from '../../app-info';
import notify from 'devextreme/ui/notify';
import { authenticationService } from '../../_services/authentication.service';
//import { Link } from 'react-router-dom';

export default class LoginForm extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      login: '',
      password: '',
      error: ''
    };
  }


  componentDidMount() {
    this.setState({
      error: this.props.loginError
    });
  }

  render() {
    const { login, password } = this.state;

    return (
      <ValidationGroup>
        <div className={'login-header'}>
          <div className={'title'}>{appInfo.title}</div>
          <div>Inicia sesion con tu usuario del sistema</div>
        </div>
        <div className={'dx-field'}>
          <TextBox
            value={login}
            onValueChanged={this.loginChanged}
            placeholder={'Usuario'}
            width={'100%'}
          >
            <Validator>
              <RequiredRule message={'Usuario requerido'} />
            </Validator>
          </TextBox>
        </div>
        <div className={'dx-field'}>
          <TextBox
            mode={'password'}
            value={password}
            onValueChanged={this.passwordChanged}
            placeholder={'Contraseña'}
            width={'100%'}
          >
            <Validator>
              <RequiredRule message={'Contraseña requerida'} />
            </Validator>
          </TextBox>
        </div>
        <div className={'dx-field'}>
          <CheckBox defaultValue={false} text={'Recuerdame'} />
        </div>
        <div className={'dx-field'}>
          <Button
            type={'default'}
            text={'Login'}
            onClick={this.onLoginClick}
            width={'100%'}
          />
        </div>
        {/* <div className={'dx-field'}>
          <Link to={'/recovery'} onClick={e => e.preventDefault()}>Forgot password ?</Link>
        </div>
        <div className={'dx-field'}>
          <Button type={'normal'} text={'Create an account'} width={'100%'} />
        </div> */}
      </ValidationGroup>
    );
  }

  loginChanged = e => {
    this.setState({ login: e.value });
  };

  passwordChanged = e => {
    this.setState({ password: e.value });
  };

  onLoginClick = args => {
    if (!args.validationGroup.validate().isValid) {
      return;
    }

    const { login, password } = this.state;
    authenticationService.login(login, password)
      .then(user => {
        this.props.onLoginClick(user);
      }, error => {
        this.setState({ error: error });
        notify(error, 'error', 2000);
      });

    //args.validationGroup.reset();
  };
}
