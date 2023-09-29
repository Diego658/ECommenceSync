import 'devextreme/dist/css/dx.common.css';
import './themes/generated/theme.base.css';
import './themes/generated/theme.additional.css';



//import "react-image-gallery/styles/scss/image-gallery.scss";
//import 'react-tabs/style/react-tabs.css';
import React, { Component } from 'react';
import { HashRouter as Router, Redirect, Route, Switch } from 'react-router-dom';
import appInfo from './app-info';
import { navigation } from './app-navigation';
import routes from './app-routes';
import './App.scss';
import './dx-styles.scss';
import { Footer, LoginForm } from './components';
import {
  SideNavOuterToolbar as SideNavBarLayout,
  SingleCard
} from './layouts';
import { sizes, subscribe, unsubscribe } from './utils/media-query';
import UserSesion from './utils/userSesion';
import { library } from '@fortawesome/fontawesome-svg-core'
import { fas } from '@fortawesome/free-solid-svg-icons'
import { fab } from '@fortawesome/free-brands-svg-icons'

import { authenticationService } from './_services/authentication.service';
import { interopConfiguracionesService } from './_services/interopConfiguraciones.service'
import './page404.scss'
import { pdfjs } from 'react-pdf';
import { CssLoadingIndicator } from './components/cssLoader/css-loader';




const PageLoading = (props) => (
  <React.Fragment>

    <div className="FourOhFour">
      <div className="bg">
        <CssLoadingIndicator />
      </div>
    </div>
  </React.Fragment>
);


const Page404 = (props) => (
  <React.Fragment>
    <div className="FourOhFour">
      <div className="bg" style={{ backgroundImage: 'url(http://i.giphy.com/l117HrgEinjIA.gif)' }}></div>
      <div className="code">404</div>
      <div className="codeMessage" >Sin conexión con el api, intente recargar la página.</div>
    </div>
  </React.Fragment>
);



const LoginContainer = ({ logIn }) => <LoginForm onLoginClick={logIn} />;


const NotAuthPage = (props) => (
  <SingleCard>
    <Route render={() => <LoginContainer {...props} />} />
  </SingleCard>
);

const AuthPage = (props) => (
  <SideNavBarLayout menuItems={navigation} title={appInfo.title} {...props}>
    <Switch>
      {routes.map(item => (
        <Route
          exact
          key={item.path}
          path={item.path}
          component={item.component}
          props={props}
        />
      ))}
      <Redirect to={'/home'} />
    </Switch>
  </SideNavBarLayout>
);

class App extends Component {
  constructor(props) {
    super(props);

    pdfjs.GlobalWorkerOptions.workerSrc = `//cdnjs.cloudflare.com/ajax/libs/pdf.js/${pdfjs.version}/pdf.worker.js`;

    library.add(fab, fas)

    this.state = {
      currentUser: null,
      screenSizeClass: this.getScreenSizeClass(),
      screenHeigth: window.innerHeight,
      isConfiguracionLoaded: false,
      configuraciones: [],
      configuracion: null,
      loginError: null,
      error: null,
      isLoading: true
    };

    this.userMenuItems = [
      {
        text: 'Perfil',
        icon: 'user'
      },
      {
        text: 'Cerrar Sesión',
        icon: 'runner',
        onClick: this.logOut
      }
    ];

    this.onConfiguracionChanged = this.onConfiguracionChanged.bind(this);

  }



  componentDidMount() {
    subscribe(this.screenSizeChanged);
    authenticationService.currentUser.subscribe(x => this.setState({ currentUser: x }));
    this.loadConfiguraciones();
  }

  componentWillUnmount() {
    unsubscribe(this.screenSizeChanged);
  }

  render() {
    const { configuraciones, configuracion, currentUser, error, isLoading, screenHeigth } = this.state;
    const loggedIn = currentUser != null;

    if (error != null && error !== 'Unauthorized') {
      return (
        <React.Fragment>
          <div>
            <Page404 />
          </div>
        </React.Fragment>
      );

    }

    if (loggedIn && (isLoading || configuracion == null)) {
      return (
        <React.Fragment>
          <div>
            <PageLoading />
          </div>
        </React.Fragment>
      );
    }


    return (
      <div className={`app ${this.state.screenSizeClass}`}>
        <Router>{loggedIn ? <AuthPage screenHeigth={screenHeigth - 250} userMenuItems={this.userMenuItems} configuraciones={configuraciones} configuracion={configuracion} onConfiguracionChanged={this.onConfiguracionChanged} /> : <NotAuthPage loginError={this.state.loginError} logIn={this.logIn} />}</Router>
      </div>
    );
  }

  onConfiguracionChanged(e) {
    this.setState({
      configuracion: e.value
    });
    UserSesion.setConfiguracion(e.value);
  }

  loadConfiguraciones() {
    this.setState({
      isLoading: true
    });

    interopConfiguracionesService.getAll()
      .then(
        (result) => {

          UserSesion.setConfiguracion(result[0]);

          this.setState({
            isConfiguracionLoaded: true,
            configuracion: result[0],
            configuraciones: result,
            isLoading: false
          });

        },

        (error) => {
          this.setState({
            isConfiguracionLoaded: false,
            error,
            isLoading: false
          });
        }
      );
  }


  getScreenSizeClass() {
    const screenSizes = sizes();
    return Object.keys(screenSizes).filter(cl => screenSizes[cl]).join(' ');
  }

  screenSizeChanged = () => {
    this.setState({
      screenSizeClass: this.getScreenSizeClass(),
      screenHeigth: window.innerHeight
    });
  }

  logIn = (currentUser) => {
    this.loadConfiguraciones();
    this.setState({ currentUser: currentUser })
  };

  logOut = () => {
    authenticationService.logout();
  };
}

export default App;
