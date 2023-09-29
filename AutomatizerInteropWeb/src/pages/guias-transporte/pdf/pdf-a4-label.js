import React from 'react';
import { Page, Text, View, Document, StyleSheet, Image } from '@react-pdf/renderer';

const styles = StyleSheet.create({
  page: {
    flexDirection: 'column',
  },
  sectionLogo: {
    margin: 'auto',
    padding: 10,
    flexGrow: 1,
    flexDirection: 'row'
  },
  subLogo: {
    flexGrow: 1,
    padding: 10,
  },
  section: {
    margin: 10,
    padding: 10,
    flexGrow: 2
  },
  sectionDePara: {
    flexDirection: 'row',
    margin: 10,
    padding: 10,
    flexGrow: 2
  },
  sectionDe: {
    margin: 10,
    padding: 10,
    flexGrow: 2
  },
  sectionPara: {
    margin: 10,
    padding: 10,
    flexGrow: 2
  },
  sectionSeparador: {
    margin: 10,
    padding: 10,
    flexGrow: 1
  },
  title: {
    fontWeight: 'extrabold',
    fontSize: 14,
    alignContent: 'center'
  },
  direcciones: {
    fontWeight: 'light',
    fontSize: 10
  },
  envioDe: {
    fontWeight: 'extrabold',
    fontSize: 14,
  },
  envioPara: {
    fontWeight: 'extrabold',
    fontSize: 12,
  }
});


export default function PdfA4Label({ deliverAddres, carrier, carrierInfo, customer, pedido }) {
  return (
    <Document>
      <Page size="A4" style={styles.page} orientation='landscape' >
        <View style={styles.section}>
          <Image src='./fragile.png'>
          </Image>
        </View>
        <View style={styles.sectionLogo} >
          <View style={styles.subLogo}>

          </View>
          <View style={styles.subLogo}>
            <Image src='./store-logo.jpg' style={{ width: 120, height: 80 }} />
          </View>
          <View style={styles.subLogo}>
            <Text style={styles.title} >Venta al por mayor de Partes y Piezas  para Portátiles.</Text>
            <Text style={styles.title}> IMPORTADOR DIRECTO</Text>
            <Text style={styles.direcciones} >Dirección: Calle Cuenca entre 24 de mayo  y 9 de octubre</Text>
            <Text style={styles.direcciones}>Mail: helpcompinfo@gmail.com Skype: helpcomp2009@hotmail.com</Text>
            <Text style={styles.direcciones}>Telf. 07 2703 610-- 07-2525018 Cell.0991084349</Text>
            <Text style={styles.direcciones}>www.helpcompartes.com</Text>
          </View>
          <View>

          </View>
        </View>
        <View style={styles.sectionDePara}>
          <View style={styles.sectionDe}>
            <Text style={styles.envioDe}>DE: CACERES SAMANIEGO CHRISTIAN </Text>
            <Text style={styles.envioDe}>RUC: 1400799845001</Text>
          </View>
          <View style={styles.sectionSeparador}>

          </View>
          <View style={styles.sectionPara}>
            <Text style={styles.envioPara}>{`PARA: ${deliverAddres.firstname} ${deliverAddres.lastname} - ${deliverAddres.company}`}</Text>
            <Text style={styles.envioPara}>{`CI/RUC: ${deliverAddres.dni}`}</Text>
            <Text style={styles.envioPara}>{`Telef. ${deliverAddres.phone_mobile} - ${deliverAddres.phone}`}</Text>
            <Text style={styles.envioPara}>{`Dir: ${deliverAddres.address1} ${deliverAddres.address2}`}</Text>
            <Text style={styles.envioPara}>{`Ref: ${pedido.reference}`}</Text>
            <Text style={styles.envioPara}>{`${deliverAddres.PrvNom} - ${deliverAddres.city}`}</Text>
          </View>
        </View>
      </Page>
    </Document>
  )
}
