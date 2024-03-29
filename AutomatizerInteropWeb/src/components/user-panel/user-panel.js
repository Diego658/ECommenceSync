import React from 'react';
import ContextMenu, { Position } from 'devextreme-react/context-menu';
import List from 'devextreme-react/list';
import './user-panel.scss';
import { authenticationService } from '../../_services/authentication.service';

export default class UserPanel extends React.Component {
  render() {
    const { menuMode, menuItems } = this.props;
    return (
      <div className={'user-panel'}>
        <div className={'user-info'}>
          <div className={'image-container'}>
            <div className={'user-image'} />
          </div>
          <div className={'user-name'}>{authenticationService.currentUserValue.username}</div>
        </div>

        {menuMode === 'context' && (
          <ContextMenu
            items={menuItems}
            target={'.user-button'}
            showEvent={'dxclick'}
            width={170}
            cssClass={'user-menu'}
          >
            <Position my={'top center'} at={'bottom center'} />
          </ContextMenu>
        )}
        {menuMode === 'list' && (
          <List className={'dx-toolbar-menu-action'} items={menuItems} />
        )}
      </div>
    );
  }
}
